using ForeverPin.Domain.Codes.Rules.Mappers;
using WoW.Two.Sdk.Backend.Beta.Codes.Payloads;
using WoW.Two.Sdk.Backend.Beta.Foundation.Options;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using ForeverPin.Domain.Codes.Serialization.Factories;
using WoW.Two.Sdk.Backend.Beta.Foundation.Serialization;
using Microsoft.Extensions.DependencyInjection;
using ForeverPin.Persistence.DataContexts;
using ForeverPin.Redirect.Api.Application.Analytics.Models;
using ForeverPin.Redirect.Api.Application.Routing.Services;
using ForeverPin.Redirect.Api.Infrastructure.Analytics;
using ForeverPin.Redirect.Api.Infrastructure.Routing;
using ForeverPin.Redirect.Api.Settings;
using WoW.Two.Sdk.Backend.Beta.Data;
using WoW.Two.Sdk.Backend.Beta.Foundation.Configuration;
using WoW.Two.Sdk.Backend.Beta.Geo.IpLocation;
using WoW.Two.Sdk.Backend.Beta.Jobs.Batches;
using WoW.Two.Sdk.Backend.Beta.Meta;
using WoW.Two.Sdk.Backend.Beta.Observability.HealthChecks;

namespace ForeverPin.Redirect.Api.Configurations;

public static partial class HostConfiguration
{
    /// <summary>Configures file settings and explicitly supported deployment aliases.</summary>
    private static WebApplicationBuilder AddConfiguration(this WebApplicationBuilder builder)
    {
        var hosting = new Dictionary<string, string?>();
        foreach (var key in new[] { "urls", "http_ports", "https_ports" })
            if (builder.Configuration[key] is { } value) hosting[key] = value;
        foreach (var source in builder.Configuration.Sources.OfType<EnvironmentVariablesConfigurationSource>().ToArray())
            builder.Configuration.Sources.Remove(source);
        builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: false);
        builder.Configuration.AddInMemoryCollection(hosting);
        var aliases = new Dictionary<string, string>
        {
            ["DB_CONNECTION"] = "DB_CONNECTION",
            ["REDIRECT_BASE_URL"] = "ApiSettings:RedirectBaseUrl",
            ["ALLOWED_HOSTS"] = "AllowedHosts",
            ["AllowedHosts"] = "AllowedHosts",
            ["IP_LOCATION_DIRECTORY"] = "IpLocation:DatabaseDirectory",
            ["DATA_PROTECTION_KEY_PATH"] = "DataProtection:KeyPath",
            ["DataProtection__KeyPath"] = "DataProtection:KeyPath",
            ["GOOGLE_CLIENT_ID"] = "Auth:Google:ClientId",
            ["BILLING_SECRET_KEY"] = "Billing:SecretKey",
            ["BILLING_WEBHOOK_SECRET"] = "Billing:WebhookSecret",
            ["BILLING_SUCCESS_URL"] = "Billing:SuccessUrl",
            ["BILLING_CANCEL_URL"] = "Billing:CancelUrl",
            ["BILLING_PRICE_SOLO"] = "Billing:Prices:Solo",
            ["BILLING_PRICE_PRO"] = "Billing:Prices:Pro",
            ["BILLING_PRICE_AGENCY"] = "Billing:Prices:Agency",
        };
        var overrides = new Dictionary<string, string?>();
        foreach (var alias in aliases)
            if (Environment.GetEnvironmentVariable(alias.Key) is { } value) overrides[alias.Value] = value;
        builder.Configuration.AddInMemoryCollection(overrides);
        return builder;
    }


    /// <summary>Trusts forwarded headers from the configured ingress addresses and networks, besides loopback.</summary>
    private static void TrustDeploymentProxies(this ApiDefaultsOptions options, IConfiguration configuration)
    {
        foreach (var proxy in configuration.GetSection("Deployment:TrustedProxies").Get<string[]>() ?? [])
            options.TrustedProxies.Add(proxy);
        foreach (var network in configuration.GetSection("Deployment:TrustedNetworks").Get<string[]>() ?? [])
            options.TrustedNetworks.Add(network);
    }

    /// <summary>Registers the database readiness probe and the redirect settings.</summary>
    private static WebApplicationBuilder AddSettings(this WebApplicationBuilder builder)
    {
        builder.Services.AddHealthChecks().AddDatabaseReadinessCheck<AppDbContext>();
        builder.Services.AddValidatedSettings<RedirectSettings>(builder.Configuration, nameof(RedirectSettings),
            options => options.Validate(settings => settings.ConfigCacheSeconds > 0,
                "RedirectSettings.ConfigCacheSeconds must be positive."));
        return builder;
    }

    /// <summary>Registers the Postgres host floor for <see cref="AppDbContext"/>.</summary>
    private static WebApplicationBuilder AddPostgresDatabase(this WebApplicationBuilder builder)
    {
        builder.Services.AddStoredJsonOptionsProfile("foreverpin-content", CodeJsonOptionsFactory.CreateContent());
        builder.Services.AddStoredJsonOptionsProfile("foreverpin-rules", CodeJsonOptionsFactory.CreateRules());
        builder.Services.AddStoredJsonOptionsProfile("foreverpin-style", CodeJsonOptionsFactory.CreateStyle());
        builder.Services.AddPostgresPersistence<AppDbContext>(builder.Configuration);
        return builder;
    }

    /// <summary>Registers the routing pipeline.</summary>
    private static WebApplicationBuilder AddRouting(this WebApplicationBuilder builder)
    {
        builder.Services.AddMemoryCache();
        builder.Services.AddCodePayloads();
        builder.Services.AddSingleton<CodePayloadMapper>();
        builder.Services.AddSingleton<WoW.Two.Sdk.Backend.Beta.Web.RequestContext.Parsers.IAcceptLanguageParser,
            WoW.Two.Sdk.Backend.Beta.Web.RequestContext.Parsers.AcceptLanguageParser>();
        builder.Services.AddSingleton<IRoutingService, RoutingService>();
        builder.Services.AddSingleton<IDeviceMapper, UserAgentDeviceMapper>();

        // Country rules read the free DB-IP Lite database, refreshed monthly; read lazily for test hosts.
        builder.Services.AddDbIpLiteIpLocation(options =>
        {
            options.DatabaseDirectory = builder.Configuration["IpLocation:DatabaseDirectory"]
                ?? Path.Combine(Path.GetTempPath(), "foreverpin", "ip-location");
            options.EnableDownload = builder.Configuration.GetValue("IpLocation:EnableDownload", true);
        });

        // Read each scan from the database so edits apply without cache invalidation.
        builder.Services.AddSingleton<IRedirectCodeRepository, DbRedirectCodeRepository>();

        // Best-effort analytics: scans queue in memory and flush in batches; a full queue drops and counts.
        builder.Services.AddBatchPipeline<ScanRecordModel, ScanBatchHandler>(options => options.Name = "scans");

        return builder;
    }
}
