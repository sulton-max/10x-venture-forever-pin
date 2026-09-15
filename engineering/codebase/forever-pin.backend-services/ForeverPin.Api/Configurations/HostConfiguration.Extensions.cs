using System.Text.Json.Serialization.Metadata;
using Microsoft.Extensions.DependencyInjection;
using ForeverPin.Application.Billing.Core.Services;
using ForeverPin.Domain.Codes.Content;
using ForeverPin.Common.Domain.Serialization.Json;
using ForeverPin.Domain.Codes.Rules.Models;
using ForeverPin.Application.Codes.Core.Services;
using ForeverPin.Application.Identity.Core.Services;
using ForeverPin.Infrastructure.Billing.Services;
using ForeverPin.Infrastructure.Codes.Core.Services;
using ForeverPin.Infrastructure.Persistence.Repositories;
using ForeverPin.Application.Settings;
using WoW.Two.Sdk.Backend.Beta.Codes;
using ForeverPin.Persistence.DataContexts;
using WoW.Two.Sdk.Backend.Beta.Data;
using WoW.Two.Sdk.Backend.Beta.Foundation.Configuration;
using WoW.Two.Sdk.Backend.Beta.Identity.Cookies;
using WoW.Two.Sdk.Backend.Beta.Identity.CurrentUser;
using WoW.Two.Sdk.Backend.Beta.Identity.Guest;
using WoW.Two.Sdk.Backend.Beta.Identity.OAuth.Google;
using WoW.Two.Sdk.Backend.Beta.Mediator;
using WoW.Two.Sdk.Backend.Beta.Mediator.Validation;
using WoW.Two.Sdk.Backend.Beta.Web.ExceptionHandling;
using WoW.Two.Sdk.Backend.Beta.Web.Json;

namespace ForeverPin.Api.Configurations;

public static partial class HostConfiguration
{
    /// <summary>Loads and registers the application settings.</summary>
    private static WebApplicationBuilder AddSettings(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton(ConfigurationLoader.Load<ApiSettings>(builder.Configuration));
        builder.Services.AddSingleton(ConfigurationLoader.Load<BillingSettings>(builder.Configuration, "Billing"));
        builder.Services.AddSingleton(ConfigurationLoader.Load<AuthSettings>(builder.Configuration, "Auth"));
        return builder;
    }

    /// <summary>Registers the Postgres host floor for <see cref="AppDbContext"/>.</summary>
    private static WebApplicationBuilder AddPersistence(this WebApplicationBuilder builder)
    {
        builder.Services.AddPostgresPersistence<AppDbContext>(builder.Configuration);
        return builder;
    }

    /// <summary>Registers the code-rendering engine and the image service.</summary>
    private static WebApplicationBuilder AddCodeServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddCodeRendering();
        builder.Services.AddScoped<ICodeImageService, CodeImageService>();
        return builder;
    }

    /// <summary>Registers the mediator pipeline and the application services.</summary>
    private static WebApplicationBuilder AddApplicationServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddMediator(typeof(ForeverPin.Infrastructure.InfrastructureAssembly).Assembly);

        // Validate before handlers; the exception filter maps failures to HTTP 400.
        builder.Services.AddMediatorValidationBehavior();

        builder.Services.AddScoped<ICodeRepository, CodeRepository>();
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
        var auth = ConfigurationLoader.Load<AuthSettings>(builder.Configuration, "Auth");
        builder.Services.AddGoogleIdTokenVerifier(o => o.WithClientId(auth.Google.ClientId));

        // API mode returns 401/403 (not a 302) so the SPA reacts; HttpOnly/Secure/SameSite=Lax come from SDK defaults.
        builder.Services.AddCookieAuthentication(o =>
        {
            o.Mode = AuthChallengeMode.Api;
            o.CookieName = "foreverpin-auth";
            o.ExpireTimeSpan = TimeSpan.FromDays(30);
            o.SlidingExpiration = true;
        });

        builder.Services.AddAuthorization();
        return builder;
    }

    /// <summary>Registers the subscription repository and the Stripe broker.</summary>
    private static WebApplicationBuilder AddBilling(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        builder.Services.AddScoped<IBillingBroker, StripeBillingBroker>();
        return builder;
    }

    /// <summary>Registers the controllers with validation filtering and string-enum JSON.</summary>
    private static WebApplicationBuilder AddControllers(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddControllers()
            .AddValidationExceptionFilter()
            .AddJsonStringEnums()
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
}
