using ForeverPin.Application.Codes.Core.Models;
using ForeverPin.Application.Codes.Core.Queries;
using ForeverPin.Application.Codes.Core.Services;
using ForeverPin.Infrastructure.Codes.Core.Extensions;
using ForeverPin.Application.Settings;
using WoW.Two.Sdk.Backend.Beta.Foundation.Errors;
using WoW.Two.Sdk.Backend.Beta.Mediator.Cqrs;
using WoW.Two.Sdk.Backend.Beta.Mediator.Result;

namespace ForeverPin.Infrastructure.Codes.Core.QueryHandlers;

/// <summary>Handles <see cref="CodeGetByIdQuery"/>.</summary>
public sealed class CodeGetByIdQueryHandler(
    ICodeRepository repository,
    ApiSettings settings)
    : IQueryHandler<CodeGetByIdQuery, AppResult<CodeGetByIdResult.Success>>
{
    /// <inheritdoc />
    public async ValueTask<AppResult<CodeGetByIdResult.Success>> HandleAsync(
        CodeGetByIdQuery request, CancellationToken ct)
    {
        var code = await repository.GetByIdForUserAsync(request.Id, request.UserId, ct);

        if (code is null)
            return AppResult<CodeGetByIdResult.Success>.Fail(AppError.Of(AppErrorType.NotFound, "Code not found"));

        return AppResult<CodeGetByIdResult.Success>.Ok(
            new CodeGetByIdResult.Success(code.ToModel(settings.RedirectBaseUrl)));
    }
}
