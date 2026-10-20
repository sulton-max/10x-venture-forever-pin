using ForeverPin.Domain.Codes.Rules.Mappers;
using WoW.Two.Sdk.Beta.Be.Codes.Payloads;
using WoW.Two.Sdk.Beta.Be.Foundation.Identifiers.Generators;
using ForeverPin.Domain.Codes.Serialization.Factories;
using WoW.Two.Sdk.Beta.Be.Foundation.Serialization;
using System.Text.Json.Serialization.Metadata;
using Microsoft.Extensions.DependencyInjection;
using ForeverPin.Application.Billing.Core.Services;
using ForeverPin.Application.Codes.Analytics.Services;
using ForeverPin.Domain.Codes.Content;
using ForeverPin.Domain.Codes.Rules.Models;
using ForeverPin.Application.Codes.Core.Services;
using ForeverPin.Application.Identity.Core.Services;
using ForeverPin.Infrastructure.Billing.Services;
using ForeverPin.Infrastructure.Codes.Core.Services;
using ForeverPin.Infrastructure.Persistence.Repositories;
using ForeverPin.Application.Settings;
using WoW.Two.Sdk.Beta.Be.Codes;
using ForeverPin.Persistence.DataContexts;
using WoW.Two.Sdk.Beta.Be.Data;
using WoW.Two.Sdk.Beta.Be.Data.Migrations.Bespoke;
using WoW.Two.Sdk.Beta.Be.Foundation.Configuration;
using WoW.Two.Sdk.Beta.Be.Foundation.Options;
using WoW.Two.Sdk.Beta.Be.Identity.Cookies;
using WoW.Two.Sdk.Beta.Be.Identity.CurrentUser;
using WoW.Two.Sdk.Beta.Be.Identity.Guest;
using WoW.Two.Sdk.Beta.Be.Identity.OAuth.Google;
using WoW.Two.Sdk.Beta.Be.Mediator;
using WoW.Two.Sdk.Beta.Be.Mediator.Validation;
using WoW.Two.Sdk.Beta.Be.Messaging.Reliability.Ef;
using WoW.Two.Sdk.Beta.Be.Meta;
using WoW.Two.Sdk.Beta.Be.Observability.HealthChecks;
using WoW.Two.Sdk.Beta.Be.Web.Antiforgery;
using WoW.Two.Sdk.Beta.Be.Web.DataProtection;
using WoW.Two.Sdk.Beta.Be.Web.ExceptionHandling;
using WoW.Two.Sdk.Beta.Be.Web.Json;

namespace ForeverPin.Api.Configurations;

public static partial class HostConfiguration
{
    /// <summary>Maps the supported deployment variables; no other variable reaches configuration.</summary>
    /// <remarks>The settings files (a target's <c>appsettings.Target.json</c>, a local run's <c>appsettings.Local.json</c>) come from <c>AddApiDefaults</c>.</remarks>
    private static WebApplicationBuilder AddConfiguration(this WebApplicationBuilder builder)
    {
        builder.Configuration.UseEnvironmentAliases(new Dictionary<string, string>
        {
            ["DB_CONNECTION"] = "DB_CONNECTION",
            ["REDIRECT_BASE_URL"] = "ApiSettings:RedirectBaseUrl",
            ["ALLOWED_HOSTS"] = "AllowedHosts",
            ["AllowedHosts"] = "AllowedHosts",
            ["DATA_PROTECTION_KEY_PATH"] = "DataProtection:KeyPath",
            ["DataProtection__KeyPath"] = "DataProtection:KeyPath",
            ["GOOGLE_CLIENT_ID"] = "Auth:Google:ClientId",
            ["BILLING_SECRET_KEY"] = "Billing:SecretKey",
            ["BILLING_WEBHOOK_SECRET"] = "Billing:WebhookSecret",
            ["BILLING_SUCCESS_URL"] = "Billing:SuccessUrl",
            ["BILLING_CANCEL_URL"] = "Billing:CancelUrl",
            ["BILLING_PORTAL_RETURN_URL"] = "Billing:PortalReturnUrl",
            ["BILLING_PRICE_BASIC"] = "Billing:Prices:Basic",
        });
        return builder;
    }


    /// <summary>Points the redirect base and the Stripe return pages at the sites a deployed target publishes.</summary>
    /// <remarks>Wheelhouse writes each published site's origin to <c>Sites:{site}:Url</c>, so no target types one.</remarks>
    private static WebApplicationBuilder AddPublishedSiteUrls(this WebApplicationBuilder builder)
    {
        var app = builder.Configuration["Sites:app:Url"]?.TrimEnd('/');
        var go = builder.Configuration["Sites:go:Url"]?.TrimEnd('/');
        var values = new Dictionary<string, string?>();
        if (!string.IsNullOrWhiteSpace(go))
            values["ApiSettings:RedirectBaseUrl"] = go;
        if (!string.IsNullOrWhiteSpace(app))
        {
            values["Billing:SuccessUrl"] = $"{app}/app/billing?status=success";
            values["Billing:CancelUrl"] = $"{app}/app/billing?status=cancelled";
            values["Billing:PortalReturnUrl"] = $"{app}/app/billing";
        }

        if (values.Count > 0)
            builder.Configuration.AddInMemoryCollection(values);
        return builder;
    }

    /// <summary>Registers persistent cookie keys and the application settings.</summary>
    private static WebApplicationBuilder AddSettings(this WebApplicationBuilder builder)
    {
        // A controller naming a budget configuration lacks would fail every such request; fail the start instead.
        var missing = RateLimitPolicies.All
            .Where(name => !builder.Configuration.GetSection($"RateLimits:Policies:{name}").Exists())
            .ToArray();
        if (missing.Length > 0)
            throw new InvalidOperationException($"RateLimits:Policies lacks {string.Join(", ", missing)}.");

        // Keys outlive container replacement, so guest capabilities and sign-ins stay valid; read lazily for test hosts.
        builder.Services.AddPersistentDataProtection(options =>
        {
            options.ApplicationName = "ForeverPin";
            options.KeyDirectory = builder.Configuration["DataProtection:KeyPath"];
        });
        builder.Services.AddValidatedSettings<ApiSettings>(builder.Configuration, nameof(ApiSettings),
            options => options.Validate(settings => IsHttpUrl(settings.RedirectBaseUrl),
                "ApiSettings.RedirectBaseUrl must be an absolute HTTP(S) URL."));
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

    /// <summary>Registers the code-rendering engine and the image service.</summary>
    private static WebApplicationBuilder AddCodes(this WebApplicationBuilder builder)
    {
        builder.Services.AddCodeRendering();
        builder.Services.AddCodePayloads();
        builder.Services.AddSingleton<CodePayloadMapper>();
        builder.Services.AddSingleton<IIdGenerator, CryptographicIdGenerator>();
        builder.Services.AddScoped<ICodeImageService, CodeImageService>();
        return builder;
    }

    /// <summary>Registers the mediator pipeline and the application services.</summary>
    private static WebApplicationBuilder AddMediator(this WebApplicationBuilder builder)
    {
        builder.Services.AddMediator(typeof(ForeverPin.Infrastructure.InfrastructureAssembly).Assembly);

        // Validate before handlers; the exception filter maps failures to HTTP 400.
        builder.Services.AddMediatorValidatingInterceptor();

        builder.Services.AddScoped<ICodeRepository, CodeRepository>();
        builder.Services.AddScoped<IScanAnalyticsRepository, ScanAnalyticsRepository>();
        builder.Services.AddSingleton<ISlugGenerator, SlugGenerator>();
        return builder;
    }

    /// <summary>Registers the current-user view and guest provisioning.</summary>
    private static WebApplicationBuilder AddIdentity(this WebApplicationBuilder builder)
    {
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddGuestSession(o => o.CookieName = "user-id");
        builder.Services.AddCurrentUser(o => o.GuestCookieName = "user-id");
        return builder;
    }

    /// <summary>Registers cookie authentication and Google ID-token verification.</summary>
    private static WebApplicationBuilder AddAuth(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IUserRepository, UserRepository>();

        // Verify Google ID tokens against the Web client id (same id the SPA uses via VITE_GOOGLE_CLIENT_ID).
        builder.Services.AddValidatedSettings<AuthSettings>(builder.Configuration, "Auth",
            options => options.Validate(settings => settings.Google is not null, "Auth.Google is required."));
        var auth = ConfigurationMapper.Load<AuthSettings>(builder.Configuration, "Auth");
        if (!string.IsNullOrWhiteSpace(auth.Google?.ClientId))
            builder.Services.AddGoogleIdTokenAuthenticator(options => options.WithClientId(auth.Google.ClientId));

        // API mode returns 401/403 (not a 302) so the SPA reacts; HttpOnly/Secure/SameSite=Lax come from SDK defaults.
        builder.Services.AddCookieAuthentication(o =>
        {
            o.Mode = AuthChallengeMode.Api;
            o.CookieName = "foreverpin-auth";
            o.ExpireTimeSpan = TimeSpan.FromDays(30);
            o.SlidingExpiration = true;
        });

        builder.Services.AddAuthorization();

        // Cookie-authenticated writes echo the readable XSRF-TOKEN cookie; Stripe's signed webhook proves itself.
        builder.Services.AddSpaAntiforgery(options => options.ExemptPathPrefixes.Add("/api/billing/webhook"));
        return builder;
    }

    /// <summary>Registers the subscription repository and the Stripe broker.</summary>
    private static WebApplicationBuilder AddBilling(this WebApplicationBuilder builder)
    {
        builder.Services.AddValidatedSettings<BillingSettings>(builder.Configuration, "Billing",
            options => options
                .Validate(settings => string.IsNullOrWhiteSpace(settings.SecretKey)
                    || (!string.IsNullOrWhiteSpace(settings.WebhookSecret)
                        && settings.Prices is { } prices && !string.IsNullOrWhiteSpace(prices.Basic)
                        && IsHttpUrl(settings.SuccessUrl) && IsHttpUrl(settings.CancelUrl) && IsHttpUrl(settings.PortalReturnUrl)),
                    "Enabled billing requires a webhook secret, the Basic price ID, and absolute HTTP(S) return URLs.")
                // The file defaults point at localhost; a paying customer must never be sent there.
                .Validate<IHostEnvironment>((settings, environment) => !environment.IsProduction()
                    || string.IsNullOrWhiteSpace(settings.SecretKey)
                    || !new[] { settings.SuccessUrl, settings.CancelUrl, settings.PortalReturnUrl }.Any(IsLoopbackUrl),
                    "Production billing must return customers to the public origin, not a loopback address."));
        builder.Services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        builder.Services.AddScoped<IBillingBroker, StripeBillingBroker>();

        // Stripe redelivers events; the inbox applies each event id once, in the effect's transaction.
        builder.Services.AddEfInbox<AppDbContext>();
        return builder;
    }

    /// <summary>Registers the controllers with the SDK wire JSON and validation filtering.</summary>
    private static WebApplicationBuilder AddControllers(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddControllersWithSdkJson()
            .AddValidationExceptionFilter()
            .AddJsonOptions(options =>
            {
                // Bind both polymorphic unions in the endpoint serializer.
                var resolver = options.JsonSerializerOptions.TypeInfoResolver ?? new DefaultJsonTypeInfoResolver();
                options.JsonSerializerOptions.TypeInfoResolver = resolver
                    .WithAddedModifier(CodeContentValueObject.Subtypes.ToJsonModifier())
                    .WithAddedModifier(CodeRuleValueObject.Subtypes.ToJsonModifier());
                options.JsonSerializerOptions.AllowOutOfOrderMetadataProperties = true;
            });
        return builder;
    }
    private static bool IsHttpUrl(string? value) => Uri.TryCreate(value, UriKind.Absolute, out var uri)
        && uri.Scheme is "http" or "https" && !string.IsNullOrWhiteSpace(uri.Host);

    private static bool IsLoopbackUrl(string? value) => Uri.TryCreate(value, UriKind.Absolute, out var uri) && uri.IsLoopback;
}
