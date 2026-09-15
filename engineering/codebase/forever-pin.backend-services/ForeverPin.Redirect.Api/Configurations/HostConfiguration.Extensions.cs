using Microsoft.Extensions.DependencyInjection;
using ForeverPin.Persistence.DataContexts;
using ForeverPin.Redirect.Api.Application.Analytics.Services;
using ForeverPin.Redirect.Api.Application.Routing.Services;
using ForeverPin.Redirect.Api.Infrastructure.Analytics;
using ForeverPin.Redirect.Api.Infrastructure.Routing;
using ForeverPin.Redirect.Api.Settings;
using WoW.Two.Sdk.Backend.Beta.Data;
using WoW.Two.Sdk.Backend.Beta.Foundation.Configuration;

namespace ForeverPin.Redirect.Api.Configurations;

public static partial class HostConfiguration
{
    /// <summary>Loads and registers the redirect settings.</summary>
    private static WebApplicationBuilder AddSettings(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton(ConfigurationLoader.Load<RedirectSettings>(builder.Configuration));
        return builder;
    }

    /// <summary>Registers the Postgres host floor for <see cref="AppDbContext"/>.</summary>
    private static WebApplicationBuilder AddPersistence(this WebApplicationBuilder builder)
    {
        builder.Services.AddPostgresPersistence<AppDbContext>(builder.Configuration);
        return builder;
    }

    /// <summary>Registers the routing pipeline.</summary>
    private static WebApplicationBuilder AddRoutingServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddMemoryCache();
        builder.Services.AddSingleton<IRoutingService, RoutingService>();
        builder.Services.AddSingleton<IDeviceMapper, UserAgentDeviceMapper>();
        builder.Services.AddSingleton<IGeoBroker, NoopGeoBroker>();

        // Read each scan from the database so edits apply without cache invalidation.
        builder.Services.AddSingleton<IRedirectCodeRepository, DbRedirectCodeRepository>();

        // Async analytics: one recorder (producer) and one hosted flusher (consumer).
        builder.Services.AddSingleton<ChannelScanRecorder>();
        builder.Services.AddSingleton<IScanRecorder>(sp => sp.GetRequiredService<ChannelScanRecorder>());
        builder.Services.AddHostedService<ScanFlushBackgroundService>();

        return builder;
    }
}
