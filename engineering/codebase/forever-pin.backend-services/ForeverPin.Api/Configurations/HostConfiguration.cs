using WoW.Two.Sdk.Backend.Beta.Data;
using WoW.Two.Sdk.Backend.Beta.Meta;

namespace ForeverPin.Api.Configurations;

/// <summary>Extends the host builder and the web application for startup wiring.</summary>
public static partial class HostConfiguration
{
    /// <summary>Configures the application builder's services.</summary>
    /// <param name="builder">The web application builder to configure.</param>
    /// <returns>The same <paramref name="builder"/> for chaining.</returns>
    public static WebApplicationBuilder Configure(this WebApplicationBuilder builder)
    {
        builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: false);

        // Lay the SDK boot floor before any product seam.
        builder.AddApiDefaults(o =>
        {
            o.ServiceName = "forever-pin-api";
            o.EnableOutputCache = false;
            o.EnableRateLimiting = false;

            // Register application validators through the SDK adapter.
            o.ValidatorAssemblies.Add(typeof(ForeverPin.Application.ApplicationAssembly).Assembly);
        });

        builder
            .AddSettings()
            .AddPersistence()
            .AddCodeServices()
            .AddApplicationServices()
            .AddIdentity()
            .AddAuth()
            .AddBilling()
            .AddControllers();

        return builder;
    }

    /// <summary>Configures the middleware pipeline and endpoints after startup tasks run.</summary>
    /// <param name="app">The built web application to configure.</param>
    /// <returns>The same <paramref name="app"/> for chaining.</returns>
    public static WebApplication Configure(this WebApplication app)
    {
        // Apply pending migrations before serving — advisory-locked, blocks once at startup.
        app.Services.MigrateBespokeOnStartupAsync().GetAwaiter().GetResult();

        // Google sign-in popups need their opener, so SPA HTML overrides the SDK isolation headers.
        // Register before static files and SDK middleware: OnStarting runs in reverse registration order.
        app.UseGisFriendlyOpenerPolicy();

        // Serve the built React SPA before the SDK pipeline so static assets short-circuit.
        app.UseDefaultFiles();
        app.UseStaticFiles();

        app.UseApiDefaults();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        // Keep unknown API requests as JSON errors; use the SPA shell only for client routes.
        app.MapFallback("/api/{**slug}", () => Results.Problem(
            statusCode: StatusCodes.Status404NotFound,
            title: "Not Found")).AllowAnonymous();
        app.MapFallbackToFile("index.html").AllowAnonymous();

        return app;
    }

    /// <summary>Overrides the SDK secure-headers floor on SPA HTML so Google Identity Services works.</summary>
    private static WebApplication UseGisFriendlyOpenerPolicy(this WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            context.Response.OnStarting(static state =>
            {
                var response = ((HttpContext)state).Response;
                var contentType = response.ContentType;
                if (contentType is not null && contentType.Contains("text/html", StringComparison.OrdinalIgnoreCase))
                {
                    response.Headers["Cross-Origin-Opener-Policy"] = "same-origin-allow-popups";
                    response.Headers.Remove("Cross-Origin-Embedder-Policy");
                }

                return Task.CompletedTask;
            }, context);

            await next();
        });

        return app;
    }
}
