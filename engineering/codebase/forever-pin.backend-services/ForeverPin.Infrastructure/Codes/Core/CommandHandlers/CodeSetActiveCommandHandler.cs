using Microsoft.Extensions.Logging;
using ForeverPin.Application.Codes.Core.Commands;
using ForeverPin.Application.Codes.Core.Models;
using ForeverPin.Application.Codes.Core.Services;
using ForeverPin.Infrastructure.Codes.Core.Extensions;
using ForeverPin.Application.Settings;
using WoW.Two.Sdk.Backend.Beta.Foundation.Errors;
using WoW.Two.Sdk.Backend.Beta.Mediator.Cqrs;
using WoW.Two.Sdk.Backend.Beta.Mediator.Result;

namespace ForeverPin.Infrastructure.Codes.Core.CommandHandlers;

/// <summary>Handles <see cref="CodeSetActiveCommand"/>.</summary>
public sealed class CodeSetActiveCommandHandler(
    ICodeRepository repository,
    ApiSettings settings,
    ILogger<CodeSetActiveCommandHandler> logger)
    : ICommandHandler<CodeSetActiveCommand, AppResult<CodeSetActiveResult.Success>>
{
    /// <inheritdoc />
    public async ValueTask<AppResult<CodeSetActiveResult.Success>> HandleAsync(
        CodeSetActiveCommand request, CancellationToken ct)
    {
        try
        {
            var code = await repository.SetActiveAsync(request.Id, request.UserId, request.IsActive, ct);

            if (code is null)
                return AppResult<CodeSetActiveResult.Success>.Fail(AppError.Of(
                    AppErrorType.NotFound,
                    "Code not found"));

            return AppResult<CodeSetActiveResult.Success>.Ok(
            new CodeSetActiveResult.Success(code.ToDto(settings.RedirectBaseUrl)));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "CodeSetActive failed for {CodeId} user {UserId}", request.Id, request.UserId);
            return AppResult<CodeSetActiveResult.Success>.Fail(AppError.Of(AppErrorType.Unexpected, ex.Message));
        }
    }
}
