using System.Text.Json;
using ForeverPin.Application.Billing.Core;
using ForeverPin.Application.Billing.Core.Services;
using ForeverPin.Application.Codes.Core.Commands;
using ForeverPin.Application.Codes.Core.Enums;
using ForeverPin.Application.Codes.Core.Models;
using ForeverPin.Application.Codes.Core.Services;
using ForeverPin.Application.Settings;
using ForeverPin.Common.Domain.Codes.Core.Enums;
using ForeverPin.Domain.Billing.Enums;
using ForeverPin.Domain.Codes.Core.Entities;
using ForeverPin.Domain.Codes.Serialization.Factories;
using ForeverPin.Infrastructure.Codes.Core.Extensions;
using ForeverPin.Persistence.DataContexts;
using Microsoft.EntityFrameworkCore;
using WoW.Two.Sdk.Beta.Be.Foundation.Errors;
using WoW.Two.Sdk.Beta.Be.Mediator.Cqrs;
using WoW.Two.Sdk.Beta.Be.Mediator.Result;
using WoW.Two.Sdk.Beta.Be.Data.EntityFrameworkCore.Locks.Extensions;

namespace ForeverPin.Infrastructure.Codes.Core.CommandHandlers;

/// <summary>Handles <see cref="CodeCreateCommand"/>.</summary>
public sealed class CodeCreateCommandHandler(
    ICodeRepository repository,
    ISubscriptionRepository subscriptions,
    ISlugGenerator slugGenerator,
    ApiSettings settings,
    AppDbContext db)
    : ICommandHandler<CodeCreateCommand, AppResult<CodeCreateResult.Success>>
{
    private const int MaxSlugAttempts = 5;

    /// <inheritdoc />
    public async ValueTask<AppResult<CodeCreateResult.Success>> HandleAsync(
        CodeCreateCommand request, CancellationToken ct)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        await db.Database.AcquireTransactionLocksAsync("owner", [request.UserId], ct);
        var subscription = await subscriptions.GetByUserAsync(request.UserId, ct);
        var plan = subscription?.Plan ?? Plan.Free;
        var cap = PlanLimitsConstants.MaxCodes(plan);

        var entity = new CodeEntity
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            Name = request.Name,
            BarcodeFormat = request.BarcodeFormat,
            IsActive = true,
            StyleJson = JsonSerializer.Serialize(request.Style, CodeJsonOptionsFactory.CreateStyle()),
            Mode = request.Mode,
            ContentType = request.ContentType,
            Rules = [.. request.Rules],
        };

        for (var attempt = 0; attempt < MaxSlugAttempts; attempt++)
        {
            ct.ThrowIfCancellationRequested();
            entity.Slug = request.Mode is ContentMode.Dynamic ? slugGenerator.Next() : null;
            var status = await repository.AddWithinLimitAsync(entity, cap, ct);

            if (status is CodeInsertStatus.PlanLimitReached)
                return AppResult<CodeCreateResult.Success>.Fail(AppError.Of(
                    AppErrorType.PaymentRequired,
                    $"Plan '{plan}' allows at most {cap} codes. Upgrade to create more."));

            if (status is CodeInsertStatus.Added)
            {
                await transaction.CommitAsync(ct);
                return AppResult<CodeCreateResult.Success>.Ok(
                    new CodeCreateResult.Success(entity.ToModel(settings.RedirectBaseUrl)));
            }
        }

        return AppResult<CodeCreateResult.Success>.Fail(AppError.Of(
            AppErrorType.Conflict, "A unique public link could not be allocated. Try again."));
    }
}
