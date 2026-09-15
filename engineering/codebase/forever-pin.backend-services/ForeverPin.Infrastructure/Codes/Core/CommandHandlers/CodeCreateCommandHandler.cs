using Microsoft.Extensions.Logging;
using ForeverPin.Application.Billing.Core;
using ForeverPin.Application.Billing.Core.Services;
using ForeverPin.Application.Codes.Core.Commands;
using ForeverPin.Application.Codes.Core.Models;
using ForeverPin.Application.Codes.Core.Services;
using ForeverPin.Infrastructure.Codes.Core.Extensions;
using ForeverPin.Application.Settings;
using WoW.Two.Sdk.Backend.Beta.Codes.Models.Style;
using ForeverPin.Domain.Billing.Enums;
using ForeverPin.Common.Domain.Codes.Core.Enums;
using ForeverPin.Domain.Codes.Core.Entities;
using WoW.Two.Sdk.Backend.Beta.Foundation.Errors;
using WoW.Two.Sdk.Backend.Beta.Mediator.Cqrs;
using WoW.Two.Sdk.Backend.Beta.Mediator.Result;

namespace ForeverPin.Infrastructure.Codes.Core.CommandHandlers;

/// <summary>Handles <see cref="CodeCreateCommand"/>.</summary>
public sealed class CodeCreateCommandHandler(
    ICodeRepository repository,
    ISubscriptionRepository subscriptions,
    ISlugGenerator slugGenerator,
    ApiSettings settings,
    ILogger<CodeCreateCommandHandler> logger)
    : ICommandHandler<CodeCreateCommand, AppResult<CodeCreateResult.Success>>
{
    /// <inheritdoc />
    public async ValueTask<AppResult<CodeCreateResult.Success>> HandleAsync(
        CodeCreateCommand request, CancellationToken ct)
    {
        try
        {
            // Enforce the owner's plan cap before allocating a slug.
            var subscription = await subscriptions.GetByUserAsync(request.UserId, ct);
            var plan = subscription?.Plan ?? Plan.Free;
            var cap = PlanLimitsConstants.MaxCodes(plan);

            if (await repository.CountByUserAsync(request.UserId, ct) >= cap)
                return AppResult<CodeCreateResult.Success>.Fail(AppError.Of(
                    AppErrorType.PaymentRequired,
                    $"Plan '{plan}' allows at most {cap} codes. Upgrade to create more."));

            // Only a dynamic code resolves through the redirect, so only a dynamic code needs a slug.
            string? slug = null;
            if (request.Mode is ContentMode.Dynamic)
            {
                do
                {
                    slug = slugGenerator.Next();
                }
                while (await repository.SlugExistsAsync(slug, ct));
            }

            var codeId = Guid.NewGuid();
            var entity = new CodeEntity
            {
                Id = codeId,
                Slug = slug,
                UserId = request.UserId,
                Name = request.Name,
                BarcodeFormat = request.BarcodeFormat,
                IsActive = true, // a new code resolves immediately — nothing gates it behind a publish step
                StyleJson = StyleSpecJson.Serialize(request.Style),
                Mode = request.Mode,
                ContentType = request.ContentType,
                // Persist content-bearing rules with the code.
                Rules = [.. request.Rules],
            };

            await repository.AddAsync(entity, ct);

            return AppResult<CodeCreateResult.Success>.Ok(
            new CodeCreateResult.Success(entity.ToDto(settings.RedirectBaseUrl)));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "CodeCreate failed for user {UserId}", request.UserId);
            return AppResult<CodeCreateResult.Success>.Fail(AppError.Of(AppErrorType.Unexpected, ex.Message));
        }
    }
}
