using ForeverPin.Application.Codes.Core.Commands;
using ForeverPin.Application.Codes.Core.Models;
using ForeverPin.Application.Codes.Core.Services;
using WoW.Two.Sdk.Beta.Be.Foundation.Errors;
using WoW.Two.Sdk.Beta.Be.Mediator.Cqrs;
using WoW.Two.Sdk.Beta.Be.Mediator.Result;

namespace ForeverPin.Infrastructure.Codes.Core.CommandHandlers;

/// <summary>Handles <see cref="CodeDeleteCommand"/>.</summary>
public sealed class CodeDeleteCommandHandler(
    ICodeRepository repository)
    : ICommandHandler<CodeDeleteCommand, AppResult<CodeDeleteResult.Success>>
{
    /// <inheritdoc />
    public async ValueTask<AppResult<CodeDeleteResult.Success>> HandleAsync(
        CodeDeleteCommand request, CancellationToken ct)
    {
        var deleted = await repository.DeleteAsync(request.Id, request.UserId, ct);

        if (!deleted)
            return AppResult<CodeDeleteResult.Success>.Fail(AppError.Of(AppErrorType.NotFound, "Code not found"));

        return AppResult<CodeDeleteResult.Success>.Ok(new CodeDeleteResult.Success());
    }
}
