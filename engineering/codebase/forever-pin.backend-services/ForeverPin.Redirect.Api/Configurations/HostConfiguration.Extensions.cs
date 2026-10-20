using ForeverPin.Domain.Codes.Rules.Mappers;
using WoW.Two.Sdk.Beta.Be.Codes.Payloads;
using WoW.Two.Sdk.Beta.Be.Foundation.Options;
using ForeverPin.Domain.Codes.Serialization.Factories;
using WoW.Two.Sdk.Beta.Be.Foundation.Serialization;
using Microsoft.Extensions.DependencyInjection;
using ForeverPin.Persistence.DataContexts;
using ForeverPin.Redirect.Api.Application.Analytics.Models;
using ForeverPin.Redirect.Api.Application.Routing.Services;
using ForeverPin.Redirect.Api.Infrastructure.Analytics;
using ForeverPin.Redirect.Api.Infrastructure.Routing;
using ForeverPin.Redirect.Api.Settings;
using WoW.Two.Sdk.Beta.Be.Caching.Invalidation;
using WoW.Two.Sdk.Beta.Be.Data;
using WoW.Two.Sdk.Beta.Be.Data.Migrations.Bespoke;
using WoW.Two.Sdk.Beta.Be.Foundation.Configuration;
using WoW.Two.Sdk.Beta.Be.Geo.IpLocation;
using WoW.Two.Sdk.Beta.Be.Jobs.Batches;
using WoW.Two.Sdk.Beta.Be.Meta;
using WoW.Two.Sdk.Beta.Be.Observability.HealthChecks;

namespace ForeverPin.Redirect.Api.Configurations;

public static partial class HostConfiguration
{
    /// <summary>Maps the supported deployment variables; no other variable reaches configuration.</summary>
    /// <remarks>The settings files (a target's <c>appsettings.Target.json</c>, a local run's <c>appsettings.Local.json</c>) come from <c>AddApiDefaults</c>.</remarks>
    private static WebApplicationBuilder AddConfiguration(this WebApplicationBuilder builder)
    {
        builder.Configuration.UseEnvironmentAliases(new Dictionary<string, string>
        {
            ["DB_CONNECTION"] = "DB_CONNECTION",
            ["ALLOWED_HOSTS"] = "AllowedHosts",
            ["AllowedHosts"] = "AllowedHosts",
            ["IP_LOCATION_DIRECTORY"] = "IpLocation:DatabaseDirectory",
            ["DATA_PROTECTION_KEY_PATH"] = "DataProtection:KeyPath",
            ["DataProtection__KeyPath"] = "DataProtection:KeyPath",
        });
        return builder;
    }

    /// <summary>Registers the redirect settings.</summary>
    private static WebApplicationBuilder AddSettings(this WebApplicationBuilder builder)
    {
        builder.Services.AddValidatedSettings<RedirectSettings>(builder.Configuration, nameof(RedirectSettings),
            options => options.Validate(settings => settings.ConfigCacheSeconds > 0,
                "RedirectSettings.ConfigCacheSeconds must be positive."));
        return builder;
    }

    /// <summary>Registers the Postgres host floor for <see cref="AppDbContext"/> and its readiness probe.</summary>
    private static WebApplicationBuilder AddPostgresDatabase(this WebApplicationBuilder builder)
    {
        builder.Services.AddStoredJsonOptionsProfile("foreverpin-content", CodeJsonOptionsFactory.CreateContent());
        builder.Services.AddStoredJsonOptionsProfile("foreverpin-rules", CodeJsonOptionsFactory.CreateRules());
        builder.Services.AddStoredJsonOptionsProfile("foreverpin-style", CodeJsonOptionsFactory.CreateStyle());
        // Applied migrations carry the product version they shipped in.
        builder.Services.AddPostgresPersistence<AppDbContext>(builder.Configuration, options => options.ConfigureMigrations =
            migrations => migrations.Version = MigrationVersionMapper.ToVersion(typeof(HostConfiguration).Assembly));
        builder.Services.AddHealthChecks().AddDatabaseReadinessCheck<AppDbContext>();
        return builder;
    }

    /// <summary>Registers the routing pipeline.</summary>
    private static WebApplicationBuilder AddRouting(this WebApplicationBuilder builder)
    {
        builder.Services.AddCodePayloads();
        builder.Services.AddSingleton<CodePayloadMapper>();
        builder.Services.AddSingleton<WoW.Two.Sdk.Beta.Be.Web.RequestContext.Parsers.IAcceptLanguageParser,
            WoW.Two.Sdk.Beta.Be.Web.RequestContext.Parsers.AcceptLanguageParser>();
        builder.Services.AddSingleton<IRoutingService, RoutingService>();
        builder.Services.AddSingleton<IDeviceMapper, UserAgentDeviceMapper>();

        // Country rules read the free DB-IP Lite database, refreshed monthly; read lazily for test hosts.
        builder.Services.AddDbIpLiteIpLocation(options =>
        {
            options.DatabaseDirectory = builder.Configuration["IpLocation:DatabaseDirectory"]
                ?? Path.Combine(Path.GetTempPath(), "foreverpin", "ip-location");
            options.EnableDownload = builder.Configuration.GetValue("IpLocation:EnableDownload", true);
        });

        // Cache scanned codes in memory; the management host's committed edits evict them on every redirect host.
        builder.Services.AddSingleton<IRedirectCodeRepository, CachedRedirectCodeRepository>();
        builder.Services.AddMemoryCacheInvalidationHandler();
        builder.Services.AddPostgresCacheInvalidation();

        // Best-effort analytics: scans queue in memory and flush in batches; a full queue drops and counts.
        builder.Services.AddBatchPipeline<ScanRecordModel, ScanBatchHandler>(options => options.Name = "scans");

        return builder;
    }
}
