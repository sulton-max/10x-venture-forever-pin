using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ForeverPin.Domain.Codes.Core.Entities;
using ForeverPin.Persistence.DataContexts;
using ForeverPin.Redirect.Api.Application.Routing.Services;

namespace ForeverPin.Redirect.Api.Infrastructure.Routing;

/// <summary>Fetches the scanned code and its rules from Postgres.</summary>
/// <remarks>Front it with <see cref="CachedRedirectCodeRepository"/> when the caching item lands.</remarks>
public sealed class DbRedirectCodeRepository(IServiceScopeFactory scopeFactory) : IRedirectCodeRepository
{
    /// <inheritdoc />
    public async Task<CodeEntity?> GetAsync(string slug, CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        return await db.Codes
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Slug == slug, ct);
    }
}
