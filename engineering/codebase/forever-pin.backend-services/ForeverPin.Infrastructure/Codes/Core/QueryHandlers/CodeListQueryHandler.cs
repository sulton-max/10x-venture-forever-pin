using ForeverPin.Application.Codes.Core.Models;
using ForeverPin.Application.Codes.Core.Queries;
using ForeverPin.Application.Codes.Core.Services;
using ForeverPin.Infrastructure.Codes.Core.Extensions;
using ForeverPin.Application.Settings;
using WoW.Two.Sdk.Beta.Be.Mediator.Cqrs;
using WoW.Two.Sdk.Beta.Be.Mediator.Result;

namespace ForeverPin.Infrastructure.Codes.Core.QueryHandlers;

/// <summary>Handles <see cref="CodeListQuery"/>.</summary>
public sealed class CodeListQueryHandler(
    ICodeRepository repository,
    ApiSettings settings)
    : IQueryHandler<CodeListQuery, AppResult<CodeListResult.Success>>
{
    /// <inheritdoc />
    public async ValueTask<AppResult<CodeListResult.Success>> HandleAsync(
        CodeListQuery request, CancellationToken ct)
    {
        var codes = await repository.ListByUserAsync(request.UserId, request.Q, ct);
        var models = codes.Select(c => c.ToModel(settings.RedirectBaseUrl)).ToList();

        return AppResult<CodeListResult.Success>.Ok(new CodeListResult.Success(models));
    }
}
