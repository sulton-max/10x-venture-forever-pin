using ForeverPin.Application.Codes.Core.Commands;
using ForeverPin.Application.Codes.Core.Enums;
using ForeverPin.Application.Codes.Core.Models;
using ForeverPin.Application.Codes.Core.Services;
using ForeverPin.Application.Settings;
using ForeverPin.Common.Domain.Codes.Core.Enums;
using ForeverPin.Domain.Billing.Entities;
using ForeverPin.Domain.Billing.Enums;
using ForeverPin.Domain.Codes.Content.Url.Models;
using ForeverPin.Domain.Codes.Core.Entities;
using ForeverPin.Domain.Codes.Core.Enums;
using ForeverPin.Domain.Codes.Rules.Models;
using ForeverPin.Infrastructure.Codes.Core.CommandHandlers;
using ForeverPin.Infrastructure.Persistence.Repositories;
using ForeverPin.Persistence.DataContexts;
using ForeverPin.Redirect.Api.Application.Analytics.Models;
using ForeverPin.Redirect.Api.Infrastructure.Analytics;
using ForeverPin.Tests.Integration.Harness;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using WoW.Two.Sdk.Beta.Be.Codes.Models.Style;
using WoW.Two.Sdk.Beta.Be.Foundation.Errors;
using WoW.Two.Sdk.Beta.Be.Mediator.Result;
using WoW.Two.Sdk.Beta.Be.Data.EntityFrameworkCore.Locks.Extensions;
using WoW.Two.Sdk.Beta.Be.Jobs.Batches;

namespace ForeverPin.Tests.Integration;

/// <summary>Exercises competing database clients and all-or-nothing product writes.</summary>
public sealed class DataAtomicityTests(ForeverPinTestDb db) : RepositoryTestBase(db)
{
    [Fact]
    public async Task Concurrent_creates_cannot_exceed_the_owner_cap()
    {
        var owner = Guid.NewGuid();
        var attempts = Enumerable.Range(0, 12).Select(async index =>
        {
            await using var context = NewContext();
            return await new CodeRepository(context).AddWithinLimitAsync(
                NewCode(owner, $"cap{index:0000}"), 3, default);
        });

        var outcomes = await Task.WhenAll(attempts);

        Assert.Equal(3, outcomes.Count(status => status is CodeInsertStatus.Added));
        Assert.Equal(9, outcomes.Count(status => status is CodeInsertStatus.PlanLimitReached));
        await using var check = NewContext();
        Assert.Equal(3, await check.Codes.CountAsync(code => code.UserId == owner));
    }

    [Fact]
    public async Task Create_reads_the_plan_after_a_concurrent_downgrade_releases_the_owner_lock()
    {
        var owner = Guid.NewGuid();
        await using (var seed = NewContext())
        {
            await new SubscriptionRepository(seed).UpsertByUserAsync(new SubscriptionEntity
            {
                Id = Guid.NewGuid(),
                UserId = owner,
                Plan = Plan.Basic,
                Status = SubscriptionStatus.Active,
                StripeCustomerId = "cus_downgrade",
                StripeSubscriptionId = "sub_downgrade",
            }, default);
            seed.Codes.AddRange(Enumerable.Range(0, 3).Select(index => NewCode(owner, $"down{index:000}")));
            await seed.SaveChangesAsync();
        }

        await using var creator = NewContext();
        await creator.Database.OpenConnectionAsync();
        var processId = ((NpgsqlConnection)creator.Database.GetDbConnection()).ProcessID;
        // A previously tracked snapshot must not override the authoritative read under the lock.
        await creator.Subscriptions.SingleAsync(subscription => subscription.UserId == owner);

        await using var downgrade = NewContext();
        await using var transaction = await downgrade.Database.BeginTransactionAsync();
        await downgrade.Database.AcquireTransactionLocksAsync("owner", [owner], default);
        var subscription = await downgrade.Subscriptions.SingleAsync(value => value.UserId == owner);
        subscription.Plan = Plan.Free;
        await downgrade.SaveChangesAsync();

        var handler = new CodeCreateCommandHandler(
            new CodeRepository(creator), new SubscriptionRepository(creator),
            new SequenceSlugGenerator("downgrd"), new ApiSettings(), creator);
        using var budget = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        var creation = handler.HandleAsync(NewRequest(owner), budget.Token).AsTask();

        try
        {
            // Observe the creator actually waiting in PostgreSQL, rather than timing the race with a delay.
            await using var observer = NewContext();
            while (!await observer.Database.SqlQuery<bool>($"""
                SELECT EXISTS (
                    SELECT 1 FROM pg_locks
                    WHERE pid = {processId} AND locktype = 'advisory' AND NOT granted
                ) AS "Value"
                """).SingleAsync(budget.Token))
            {
                Assert.False(creation.IsCompleted, "Create must wait on the subscription owner's lock.");
                await Task.Delay(10, budget.Token);
            }

            await transaction.CommitAsync(budget.Token);
            var failure = Assert.IsType<AppResult<CodeCreateResult.Success>.Failure>(await creation);
            Assert.Equal(AppErrorType.PaymentRequired, failure.Error.Type);
            Assert.Contains("Free", failure.Error.Message);
        }
        finally
        {
            if (!creation.IsCompleted)
            {
                await budget.CancelAsync();
                try { await creation; }
                catch (OperationCanceledException) { }
            }
        }

        await using var check = NewContext();
        Assert.Equal(3, await check.Codes.CountAsync(code => code.UserId == owner));
    }

    [Fact]
    public async Task Slug_retry_keeps_the_callers_transaction_and_prior_writes_uncommitted()
    {
        await using (var seed = NewContext())
            await new CodeRepository(seed).AddAsync(NewCode(Guid.NewGuid(), "taken03"), default);

        await using var context = NewContext();
        await using var transaction = await context.Database.BeginTransactionAsync();
        var repository = new CodeRepository(context);
        await repository.AddAsync(NewCode(Guid.NewGuid(), "prior03"), default);
        var candidate = NewCode(Guid.NewGuid(), "taken03");

        Assert.Equal(CodeInsertStatus.SlugConflict,
            await repository.AddWithinLimitAsync(candidate, 3, default));
        Assert.Equal(2, await context.Codes.CountAsync());
        Assert.Equal(EntityState.Detached, context.Entry(candidate).State);
        candidate.Slug = "fresh03";
        Assert.Equal(CodeInsertStatus.Added,
            await repository.AddWithinLimitAsync(candidate, 3, default));

        await using (var observer = NewContext())
            Assert.Equal(1, await observer.Codes.CountAsync());

        await transaction.RollbackAsync();
        await using var check = NewContext();
        Assert.Equal(1, await check.Codes.CountAsync());
    }

    [Fact]
    public async Task Slug_conflict_detaches_the_failed_insert_and_allows_a_retry()
    {
        await using var context = NewContext();
        var repository = new CodeRepository(context);
        await repository.AddAsync(NewCode(Guid.NewGuid(), "taken01"), default);
        var candidate = NewCode(Guid.NewGuid(), "taken01");

        Assert.Equal(CodeInsertStatus.SlugConflict,
            await repository.AddWithinLimitAsync(candidate, 3, default));
        Assert.Equal(EntityState.Detached, context.Entry(candidate).State);

        candidate.Slug = "fresh01";
        Assert.Equal(CodeInsertStatus.Added,
            await repository.AddWithinLimitAsync(candidate, 3, default));
        Assert.Equal(2, await context.Codes.CountAsync());
    }

    [Fact]
    public async Task Create_retries_collisions_and_stops_at_the_attempt_bound()
    {
        await using var context = NewContext();
        var repository = new CodeRepository(context);
        await repository.AddAsync(NewCode(Guid.NewGuid(), "taken02"), default);
        var generator = new SequenceSlugGenerator("taken02", "fresh02");
        var handler = new CodeCreateCommandHandler(
            repository, new SubscriptionRepository(context), generator, new ApiSettings(), context);
        var request = NewRequest(Guid.NewGuid());

        Assert.IsType<AppResult<CodeCreateResult.Success>.Success>(
            await handler.HandleAsync(request, default));
        Assert.Equal(2, generator.Calls);

        generator = new SequenceSlugGenerator("taken02");
        handler = new CodeCreateCommandHandler(
            repository, new SubscriptionRepository(context), generator, new ApiSettings(), context);
        var failure = Assert.IsType<AppResult<CodeCreateResult.Success>.Failure>(
            await handler.HandleAsync(NewRequest(Guid.NewGuid()), default));
        Assert.Equal(AppErrorType.Conflict, failure.Error.Type);
        Assert.Equal(5, generator.Calls);
        Assert.Equal(2, await context.Codes.CountAsync());
    }

    [Fact]
    public async Task Update_rejects_a_replacement_instance_instead_of_silently_ignoring_it()
    {
        await using var context = NewContext();
        var repository = new CodeRepository(context);
        var tracked = await repository.AddAsync(NewCode(Guid.NewGuid(), "track01"), default);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            repository.UpdateAsync(tracked with { Name = "replacement" }, default));

        await using var check = NewContext();
        Assert.Equal("Test", (await check.Codes.SingleAsync()).Name);
    }

    [Fact]
    public async Task Concurrent_subscription_upserts_keep_one_row_and_one_identity()
    {
        var owner = Guid.NewGuid();
        var attempts = Enumerable.Range(0, 8).Select(async index =>
        {
            await using var context = NewContext();
            return await new SubscriptionRepository(context).UpsertByUserAsync(new SubscriptionEntity
            {
                Id = Guid.NewGuid(),
                UserId = owner,
                Plan = Plan.Basic,
                Status = SubscriptionStatus.Active,
                StripeCustomerId = $"cus_{index}",
                StripeSubscriptionId = $"sub_{index}",
            }, default);
        });

        var results = await Task.WhenAll(attempts);
        Assert.Single(results.Select(subscription => subscription.Id).Distinct());
        await using var check = NewContext();
        Assert.Single(await check.Subscriptions.ToListAsync());
    }

    [Fact]
    public async Task Failed_counter_increment_rolls_back_events_and_prior_counter_updates()
    {
        var first = NewCode(Guid.NewGuid(), "scan001");
        first.Id = Guid.Parse("00000000-0000-0000-0000-000000000001");
        var overflowing = NewCode(Guid.NewGuid(), "scan002");
        overflowing.Id = Guid.Parse("00000000-0000-0000-0000-000000000002");
        overflowing.ScanCount = long.MaxValue;
        await using (var seed = NewContext())
        {
            seed.Codes.AddRange(first, overflowing);
            await seed.SaveChangesAsync();
        }

        await using (var context = NewContext())
        {
            var exception = await Assert.ThrowsAsync<PostgresException>(() =>
                new ScanBatchHandler(context).HandleAsync([NewScan(first.Id), NewScan(overflowing.Id)], default));
            Assert.Equal(PostgresErrorCodes.NumericValueOutOfRange, exception.SqlState);
        }

        await using var check = NewContext();
        Assert.Empty(await check.ScanEvents.ToListAsync());
        Assert.Equal(0, (await check.Codes.SingleAsync(code => code.Id == first.Id)).ScanCount);
        Assert.Equal(long.MaxValue, (await check.Codes.SingleAsync(code => code.Id == overflowing.Id)).ScanCount);
    }

    [Fact]
    public async Task Graceful_worker_shutdown_drains_every_accepted_batch()
    {
        var code = NewCode(Guid.NewGuid(), "drain01");
        await using (var seed = NewContext())
            await new CodeRepository(seed).AddAsync(code, default);

        var services = new ServiceCollection().AddLogging();
        services.AddScoped(_ => NewContext());
        services.AddBatchPipeline<ScanRecordModel, ScanBatchHandler>(options => options.Name = "scans");
        await using var provider = services.BuildServiceProvider();
        var pipeline = provider.GetRequiredService<BoundedBatchPipeline<ScanRecordModel>>();
        var worker = provider.GetRequiredService<IHostedService>();
        for (var i = 0; i < 250; i++)
            Assert.True(pipeline.TryWrite(NewScan(code.Id)));

        await worker.StartAsync(default);
        using var budget = new CancellationTokenSource(TimeSpan.FromSeconds(15));
        await worker.StopAsync(budget.Token);

        Assert.Equal(250, pipeline.ProcessedCount);
        Assert.Equal(0, pipeline.FailedCount);
        Assert.Equal(0, pipeline.DroppedCount);
        await using var check = NewContext();
        Assert.Equal(250, await check.ScanEvents.CountAsync());
        Assert.Equal(250, (await check.Codes.SingleAsync()).ScanCount);
    }

    private static CodeEntity NewCode(Guid owner, string slug) => new()
    {
        Id = Guid.NewGuid(),
        UserId = owner,
        Slug = slug,
        Name = "Test",
        BarcodeFormat = BarcodeFormat.QrCode,
        StyleJson = "{}",
        Mode = ContentMode.Dynamic,
        ContentType = CodeContentType.Url,
        IsActive = true,
        Rules = [new DefaultRuleValueObject { Content = new UrlContentValueObject { Url = "https://example.com" } }],
    };

    private static CodeCreateCommand NewRequest(Guid owner) => new()
    {
        UserId = owner,
        Name = "Test",
        Mode = ContentMode.Dynamic,
        ContentType = CodeContentType.Url,
        Style = StyleSpec.Default,
        Rules = [new DefaultRuleValueObject { Content = new UrlContentValueObject { Url = "https://example.com" } }],
    };

    private static ScanRecordModel NewScan(Guid code) => new()
    {
        CodeId = code,
        ScannedAt = new DateTimeOffset(2026, 9, 26, 0, 0, 0, TimeSpan.Zero),
        DestinationUrl = "https://example.com",
    };

    private sealed class SequenceSlugGenerator(params string[] slugs) : ISlugGenerator
    {
        public int Calls { get; private set; }

        public string Next() => slugs[Math.Min(Calls++, slugs.Length - 1)];
    }
}
