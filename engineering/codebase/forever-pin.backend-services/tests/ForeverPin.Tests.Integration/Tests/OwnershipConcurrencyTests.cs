using ForeverPin.Common.Domain.Codes.Core.Enums;
using ForeverPin.Domain.Codes.Content.Url.Models;
using ForeverPin.Domain.Codes.Core.Entities;
using ForeverPin.Domain.Codes.Core.Enums;
using ForeverPin.Domain.Codes.Rules.Models;
using ForeverPin.Infrastructure.Persistence.Repositories;
using ForeverPin.Persistence.DataContexts;
using ForeverPin.Tests.Integration.Harness;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ForeverPin.Tests.Integration;

/// <summary>Protects account-owned codes from guest writes crossing an ownership transfer.</summary>
public sealed class OwnershipConcurrencyTests(ForeverPinTestDb db) : RepositoryTestBase(db)
{
    [Fact]
    public async Task Guest_update_loaded_before_claim_cannot_modify_the_claimed_code()
    {
        var guest = Guid.NewGuid();
        var account = Guid.NewGuid();
        var code = await SeedCodeAsync(guest, "claim01");
        await using var guestContext = NewContext();
        var repository = new CodeRepository(guestContext);
        var tracked = await repository.GetByIdForUserAsync(code.Id, guest, default);
        Assert.NotNull(tracked);

        await ClaimAsync(guest, account);
        tracked.Name = "A stale guest write";
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => repository.UpdateAsync(tracked, default));

        await using var check = NewContext();
        var claimed = await check.Codes.SingleAsync();
        Assert.Equal(account, claimed.UserId);
        Assert.Equal("Original", claimed.Name);
        Assert.False(await new CodeRepository(check).DeleteAsync(code.Id, guest, default));
        Assert.True(await check.Codes.AnyAsync(value => value.Id == code.Id));
    }

    [Fact]
    public async Task Guest_toggle_loaded_before_claim_cannot_disable_the_claimed_code()
    {
        var guest = Guid.NewGuid();
        var account = Guid.NewGuid();
        var code = await SeedCodeAsync(guest, "claim02");
        var gate = new SaveGateInterceptor();
        var options = new DbContextOptionsBuilder<AppDbContext>();
        Db.ApplyProvider(options);
        options.AddInterceptors(gate);
        await using var guestContext = new AppDbContext(options.Options);
        using var budget = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        var toggle = new CodeRepository(guestContext).SetActiveAsync(code.Id, guest, false, budget.Token);

        try
        {
            await gate.Entered.Task.WaitAsync(budget.Token);
            await ClaimAsync(guest, account);
            gate.Release.TrySetResult();
            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => toggle);
        }
        finally
        {
            gate.Release.TrySetResult();
            try { await toggle; }
            catch (DbUpdateConcurrencyException) { }
        }

        await using var check = NewContext();
        var claimed = await check.Codes.SingleAsync();
        Assert.Equal(account, claimed.UserId);
        Assert.True(claimed.IsActive);
    }

    private async Task ClaimAsync(Guid guest, Guid account)
    {
        await using var context = NewContext();
        await using var transaction = await context.Database.BeginTransactionAsync();
        await new CodeRepository(context).ReassignOwnerAsync(guest, account, default);
        await transaction.CommitAsync();
    }

    private async Task<CodeEntity> SeedCodeAsync(Guid guest, string slug)
    {
        await using var context = NewContext();
        return await new CodeRepository(context).AddAsync(new CodeEntity
        {
            Id = Guid.NewGuid(),
            UserId = guest,
            Slug = slug,
            Name = "Original",
            BarcodeFormat = BarcodeFormat.QrCode,
            StyleJson = "{}",
            Mode = ContentMode.Dynamic,
            ContentType = CodeContentType.Url,
            IsActive = true,
            Rules = [new DefaultRuleValueObject { Content = new UrlContentValueObject { Url = "https://example.com" } }],
        }, default);
    }

    private sealed class SaveGateInterceptor : SaveChangesInterceptor
    {
        public TaskCompletionSource Entered { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource Release { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            Entered.TrySetResult();
            await Release.Task.WaitAsync(cancellationToken);
            return result;
        }
    }
}
