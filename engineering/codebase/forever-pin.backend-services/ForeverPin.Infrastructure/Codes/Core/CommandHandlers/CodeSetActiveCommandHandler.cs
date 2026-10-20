using ForeverPin.Application.Codes.Core.Commands;
using ForeverPin.Application.Codes.Core.Models;
using ForeverPin.Application.Codes.Core.Services;
using ForeverPin.Infrastructure.Codes.Core.Extensions;
using ForeverPin.Application.Settings;
using WoW.Two.Sdk.Beta.Be.Foundation.Errors;
using WoW.Two.Sdk.Beta.Be.Mediator.Cqrs;
using WoW.Two.Sdk.Beta.Be.Mediator.Result;

namespace ForeverPin.Infrastructure.Codes.Core.CommandHandlers;

/// <summary>Handles <see cref="CodeSetActiveCommand"/>.</summary>
public sealed class CodeSetActiveCommandHandler(
    ICodeRepository repository,
    ApiSettings settings)
    : ICommandHandler<CodeSetActiveCommand, AppResult<CodeSetActiveResult.Success>>
{
    /// <inheritdoc />
    public async ValueTask<AppResult<CodeSetActiveResult.Success>> HandleAsync(
        CodeSetActiveCommand request, CancellationToken ct)
    {
        var code = await repository.SetActiveAsync(request.Id, request.UserId, request.IsActive, ct);

        if (code is null)
            return AppResult<CodeSetActiveResult.Success>.Fail(AppError.Of(
                AppErrorType.NotFound,
                "Code not found"));

        return AppResult<CodeSetActiveResult.Success>.Ok(
            new CodeSetActiveResult.Success(code.ToModel(settings.RedirectBaseUrl)));
    }
}
