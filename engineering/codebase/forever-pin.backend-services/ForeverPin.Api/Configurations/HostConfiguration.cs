using WoW.Two.Sdk.Backend.Beta.Data;
using WoW.Two.Sdk.Backend.Beta.Meta;
using WoW.Two.Sdk.Backend.Beta.Web.Antiforgery;

namespace ForeverPin.Api.Configurations;

/// <summary>Extends the host builder and the web application for startup wiring.</summary>
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
            o.ServiceName = "forever-pin-api";
            o.EnableOutputCache = false;
            o.EnableRateLimiting = false;
            o.TrustProxiesFrom(builder.Configuration.GetSection("Deployment"));

            // Google sign-in popups keep their opener; its cross-origin script ships no CORP opt-in.
            o.AllowCrossOriginOpenerPopups = true;
            o.EnableCrossOriginEmbedderPolicy = false;

            // Register application validators through the SDK adapter.
            o.ValidatorAssemblies.Add(typeof(ForeverPin.Application.ApplicationAssembly).Assembly);

            // Serve the built SPA behind the secure headers and compression; each route may ship its own document.
            o.SpaHosting = _ => { };
        });

        builder
            .AddSettings()
            .AddPostgresDatabase()
            .AddCodes()
            .AddMediator()
            .AddIdentity()
            .AddAuth()
            .AddBilling()
            .AddControllers();

        return builder;
    }

    /// <summary>Configures the middleware pipeline and endpoints after startup tasks run.</summary>
    /// <param name="app">The built web application to configure.</param>
    /// <returns>The same <paramref name="app"/> for chaining.</returns>
    public static async Task<WebApplication> ConfigureAsync(this WebApplication app)
    {
        // Apply pending migrations before serving — advisory-locked, blocks once at startup.
        await app.Services.MigrateBespokeOnStartupAsync(app.Lifetime.ApplicationStopping);

        app.UseApiDefaults(pipeline =>
        {
            pipeline.UseAuthentication();
            pipeline.UseAuthorization();
            pipeline.UseSpaAntiforgery();   // after authentication: tokens bind to the signed-in user
        });

        // Unknown API requests stay JSON errors; every other unmatched route gets its document or the SPA shell.
        app.MapControllers();

        return app;
    }
}
