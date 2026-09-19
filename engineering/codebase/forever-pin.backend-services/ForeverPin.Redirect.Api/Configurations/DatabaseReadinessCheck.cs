using ForeverPin.Persistence.DataContexts;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ForeverPin.Redirect.Api.Configurations;

/// <summary>Requires the product database to be reachable before a rollout passes.</summary>
public sealed class DatabaseReadinessCheck(IServiceScopeFactory scopes) : IHealthCheck
{
    /// <inheritdoc />
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        using var scope = scopes.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await database.Database.CanConnectAsync(cancellationToken)
            ? HealthCheckResult.Healthy()
            : HealthCheckResult.Unhealthy("Database unavailable.");
    }
}
