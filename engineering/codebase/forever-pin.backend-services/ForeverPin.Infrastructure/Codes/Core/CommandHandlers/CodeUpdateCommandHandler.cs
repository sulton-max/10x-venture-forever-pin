using ForeverPin.Application.Codes.Core.Commands;
using ForeverPin.Application.Codes.Core.Models;
using ForeverPin.Application.Codes.Core.Services;
using ForeverPin.Infrastructure.Codes.Core.Extensions;
using ForeverPin.Application.Settings;
using System.Text.Json;
using ForeverPin.Domain.Codes.Serialization.Factories;
using ForeverPin.Application.Codes.Rules.Models;
using WoW.Two.Sdk.Backend.Beta.Foundation.Validation;
using WoW.Two.Sdk.Backend.Beta.Foundation.Errors;
using WoW.Two.Sdk.Backend.Beta.Mediator.Cqrs;
using WoW.Two.Sdk.Backend.Beta.Mediator.Result;
using WoW.Two.Sdk.Backend.Beta.Foundation.Validation.Trackers;
using WoW.Two.Sdk.Backend.Beta.Foundation.Validation.Trackers.Extensions;

namespace ForeverPin.Infrastructure.Codes.Core.CommandHandlers;

/// <summary>Handles <see cref="CodeUpdateCommand"/>.</summary>
public sealed class CodeUpdateCommandHandler(
    ICodeRepository repository,
    ApiSettings settings,
    IValidator<CodeUpdateModel> validator,
    IValidationAdvisoryTracker advisories)
    : ICommandHandler<CodeUpdateCommand, AppResult<CodeUpdateResult.Success>>
{
    /// <inheritdoc />
    public async ValueTask<AppResult<CodeUpdateResult.Success>> HandleAsync(
        CodeUpdateCommand request, CancellationToken ct)
    {
        var code = await repository.GetByIdForUserAsync(request.Id, request.UserId, ct);

        if (code is null)
            return AppResult<CodeUpdateResult.Success>.Fail(AppError.Of(AppErrorType.NotFound, "Code not found"));

        // Errors block the update; warnings and suggestions travel with the saved code.
        var validation = validator.Validate(new CodeUpdateModel(code.Mode, request), advisories);
        if (validation is not null)
            return AppResult<CodeUpdateResult.Success>.Fail(validation);

        // Apply editable fields — slug, scan count, and creation timestamp are deliberately untouched.
        code.Name = request.Name;
        code.BarcodeFormat = request.BarcodeFormat;

        // Full replace: the request always carries the whole style block, so there is nothing to preserve.
        code.StyleJson = JsonSerializer.Serialize(request.Style, CodeJsonOptionsFactory.CreateStyle());

        // Full replace of the rule set; mode is absent from the update contract, so it can never change.
        code.ContentType = request.ContentType;
        code.Rules = [.. request.Rules];

        var updated = await repository.UpdateAsync(code, ct);

        return AppResult<CodeUpdateResult.Success>.Ok(
            new CodeUpdateResult.Success(updated.ToModel(settings.RedirectBaseUrl)));
    }
}
