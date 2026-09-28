using ForeverPin.Redirect.Api.Endpoints;
using WoW.Two.Sdk.Backend.Beta.Data;
using WoW.Two.Sdk.Backend.Beta.Meta;

namespace ForeverPin.Redirect.Api.Configurations;

/// <summary>Extends the host builder for service and endpoint wiring.</summary>
public static partial class HostConfiguration
{
    /// <summary>Configures the application builder's services.</summary>
    /// <param name="builder">The web application builder to configure.</param>
    /// <returns>The same <paramref name="builder"/> for chaining.</returns>
    public static WebApplicationBuilder Configure(this WebApplicationBuilder builder)
    {
        builder.AddConfiguration();

        // Lay the SDK boot floor before any product seam.
        builder.AddApiDefaults(o =>
        {
            o.ServiceName = "forever-pin-redirect";
            o.EnableOutputCache = false;
            o.EnableRateLimiting = false;
            o.TrustProxiesFrom(builder.Configuration.GetSection("Deployment"));
        });

        builder
            .AddSettings()
            .AddPostgresDatabase()
            .AddRouting();

        return builder;
    }

    /// <summary>Runs startup tasks, then configures endpoints.</summary>
    /// <param name="app">The built web application to configure.</param>
    /// <returns>The same <paramref name="app"/> for chaining.</returns>
    public static async Task<WebApplication> ConfigureAsync(this WebApplication app)
    {
        // Apply pending migrations before serving — advisory-locked, blocks once at startup.
        await app.Services.MigrateBespokeOnStartupAsync(app.Lifetime.ApplicationStopping);

        app.UseApiDefaults();

        // The single hot-path route: GET /{slug} resolves rules and returns a 302.
        app.MapRedirect();

        return app;
    }
}
