extern alias apihost;
extern alias redirecthost;

using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ForeverPin.Application.Billing.Core.Services;
using ForeverPin.Persistence.DataContexts;
using Testcontainers.PostgreSql;
using WoW.Two.Sdk.Backend.Beta.Identity.OAuth.Google.Authenticators;
using WoW.Two.Sdk.Backend.Beta.Identity.CurrentUser;
using WoW.Two.Sdk.Backend.Beta.Data.EntityFrameworkCore.Audit;
using WoW.Two.Sdk.Backend.Beta.Geo.IpLocation.Brokers;
using WoW.Two.Sdk.Backend.Beta.Testing;
using WoW.Two.Sdk.Backend.Beta.Testing.Containers.Postgres;
using WoW.Two.Sdk.Backend.Beta.Testing.MultiHost;
using WoW.Two.Sdk.Backend.Beta.Testing.Web;

// Disambiguate the two top-level `Program` types (both live in the global namespace of their assembly).
using ApiProgram = apihost::Program;
using RedirectProgram = redirecthost::Program;
// Billing config types — aliased to the API settings classes (vs the unrelated SDK Billing namespace).
using BillingSettings = ForeverPin.Application.Settings.BillingSettings;
using BillingPricesSettings = ForeverPin.Application.Settings.BillingPricesSettings;

namespace ForeverPin.Tests.E2E.Harness;

/// <summary>Boots the Api and Redirect hosts over a shared Postgres container, Respawn-reset between tests.</summary>
public sealed class AppFixture : MultiHostFixture, IAsyncLifetime
{
    /// <summary>Holds the name of the identity cookie the Api host sets on guest provisioning.</summary>
    public const string UserIdCookieName = "user-id";

    /// <summary>Holds the name of the session cookie the Api host sets on Google sign-in.</summary>
    public const string AuthCookieName = "foreverpin-auth";

    /// <summary>Holds the redirect base for each code's <c>shortUrl</c>, set via <c>REDIRECT_BASE_URL</c>.</summary>
    public const string RedirectBaseUrl = "https://redirect.foreverpin.test";

    /// <summary>Holds the fake Stripe price id for the Solo plan.</summary>
    public const string PriceSolo = "price_solo";

    /// <summary>Holds the fake Stripe price id for the Pro plan.</summary>
    public const string PricePro = "price_pro";

    /// <summary>Holds the fake Stripe price id for the Agency plan.</summary>
    public const string PriceAgency = "price_agency";

    private readonly PostgresFixture _postgres;
    private readonly string _keyPath = Path.Combine(Path.GetTempPath(), "foreverpin-e2e-keys", Guid.NewGuid().ToString("N"));

    /// <summary>Gets the fake Stripe broker wired into the Api host.</summary>
    public FakeBillingBroker Gateway { get; } = new();

    /// <summary>Gets the shared Postgres fixture.</summary>
    public PostgresFixture Postgres => _postgres;

    /// <summary>Gets the management Api host.</summary>
    public WebApiTestHost<ApiProgram> ApiHost { get; }

    /// <summary>Gets the redirect (hot-path) host.</summary>
    public WebApiTestHost<RedirectProgram> RedirectHost { get; }

    /// <summary>Registers the shared container and both hosts.</summary>
    public AppFixture()
    {
        _postgres = AddSharedFixture(
            new PostgresFixture(new PostgreSqlBuilder().WithImage("postgres:16-alpine").Build()));

        ApiHost = AddHost(new WebApiTestHost<ApiProgram>
        {
            ConfigureConfigurationHook = ConfigureHost,
            // Swap the real external seams for deterministic fakes so E2E never calls Google or Stripe.
            ConfigureServicesHook = services =>
            {
                services.RemoveAll<IGoogleIdTokenAuthenticator>();
                services.AddScoped<IGoogleIdTokenAuthenticator>(_ => new FakeGoogleTokenAuthenticator());

                // Fake billing broker (no network) shared across the run so a test can stage a webhook event.
                services.RemoveAll<IBillingBroker>();
                services.AddSingleton<IBillingBroker>(Gateway);

                // Replace the configured billing singleton with deterministic Stripe price ids.
                services.RemoveAll<BillingSettings>();
                services.AddSingleton(new BillingSettings
                {
                    SecretKey = "sk_test_fake",
                    WebhookSecret = "whsec_fake",
                    Prices = new BillingPricesSettings { Solo = PriceSolo, Pro = PricePro, Agency = PriceAgency },
                    SuccessUrl = "https://app.example/ok",
                    CancelUrl = "https://app.example/cancel",
                });
            },
        });

        RedirectHost = AddHost(new WebApiTestHost<RedirectProgram>
        {
            ConfigureConfigurationHook = ConfigureHost,
            // Locate scans without downloading the DB-IP database.
            ConfigureServicesHook = services =>
            {
                services.RemoveAll<IIpLocationBroker>();
                services.AddSingleton<IIpLocationBroker, FakeIpLocationBroker>();
            },
        });
    }

    /// <summary>Holds the path whose GET issues the SPA antiforgery token to test clients.</summary>
    public const string TokenPath = "/health";

    /// <summary>Creates a fresh anonymous client against the Api host that echoes antiforgery tokens like the SPA.</summary>
    public HttpClient CreateApiClient() => ApiHost.CreateSpaClient(TokenPath);

    /// <summary>Creates a fresh client against the Redirect host that does not follow redirects.</summary>
    public HttpClient CreateRedirectClient() => RedirectHost.CreateClient(
        new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false,
    });

    /// <summary>Creates a new database context on the shared container.</summary>
    /// <remarks>Use it to seed rows the API cannot create.</remarks>
    public AppDbContext NewDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(_postgres.ConnectionString)
            .UseSnakeCaseNamingConvention()
            .AddInterceptors(new AuditInterceptor(ApiHost.Clock));
        return new AppDbContext(options.Options);
    }

    /// <summary>POSTs a raw body with a <c>Stripe-Signature</c> header to the webhook endpoint.</summary>
    public static Task<HttpResponseMessage> PostWebhookAsync(HttpClient client, string signature = "test-sig")
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/billing/webhook")
        {
            Content = new StringContent("{}", Encoding.UTF8, "application/json"),
        };
        request.Headers.TryAddWithoutValidation("Stripe-Signature", signature);
        return client.SendAsync(request);
    }

    private void ConfigureHost(IConfigurationBuilder configuration) =>
        configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["DatabaseSettings:ConnectionString"] = _postgres.ConnectionString,
            ["DB_CONNECTION"] = _postgres.ConnectionString,
            ["ApiSettings:RedirectBaseUrl"] = RedirectBaseUrl,
            ["REDIRECT_BASE_URL"] = RedirectBaseUrl,
            ["REDIS_CONNECTION"] = "",
            ["Auth:Google:ClientId"] = "foreverpin-e2e.apps.googleusercontent.com",
            ["DataProtection:KeyPath"] = _keyPath,
            ["IpLocation:DatabaseDirectory"] = _keyPath,
            ["IpLocation:EnableDownload"] = "false",
        });

    /// <summary>Snapshots the post-migration schema for Respawn.</summary>
    protected override ValueTask InitializeStateAsync(CancellationToken cancellationToken = default) =>
        _postgres.InitializeRespawnerAsync(cancellationToken);

    /// <summary>Provisions a guest and returns an Api client carrying the <c>user-id</c> cookie.</summary>
    /// <remarks>The <c>Secure</c> cookie won't round-trip over <c>http://</c>, so it's sent as a raw header.</remarks>
    public async Task<GuestClient> CreateGuestClientAsync()
    {
        using var client = ApiHost.CreateSpaClient(TokenPath);

        using var response = await client.PostAsync("/api/identity/guest", content: null);
        response.EnsureSuccessStatusCode();

        var cookie = ExtractCookie(response, UserIdCookieName)
            ?? throw new InvalidOperationException("POST /api/identity/guest did not set the user-id cookie.");

        var authed = ApiHost.CreateSpaClient(TokenPath);
        authed.DefaultRequestHeaders.Add("Cookie", $"{UserIdCookieName}={cookie}");

        return new GuestClient { Client = authed, UserId = ReadGuestId(cookie), Cookie = cookie };
    }

    private Guid ReadGuestId(string cookie)
    {
        using var scope = ApiHost.Services.CreateScope();
        var accessor = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
        var previous = accessor.HttpContext;
        try
        {
            var context = new DefaultHttpContext();
            context.Request.Headers.Cookie = $"{UserIdCookieName}={cookie}";
            accessor.HttpContext = context;
            return scope.ServiceProvider.GetRequiredService<ICurrentUserService>().Id
                ?? throw new InvalidOperationException("The issued guest cookie did not resolve through the host identity service.");
        }
        finally
        {
            accessor.HttpContext = previous;
        }
    }

    /// <summary>Lifts a cookie value out of the response's <c>Set-Cookie</c> headers, or null when absent.</summary>
    public static string? ExtractCookie(HttpResponseMessage response, string name)
    {
        if (!response.Headers.TryGetValues("Set-Cookie", out var cookies))
            return null;

        var prefix = $"{name}=";
        foreach (var cookie in cookies)
        {
            // e.g. "foreverpin-auth=<value>; path=/; secure; httponly; ..."
            var head = cookie.Split(';', 2)[0].Trim();
            if (head.StartsWith(prefix, StringComparison.Ordinal))
                return head[prefix.Length..];
        }

        return null;
    }

    /// <summary>Starts the topology for xUnit.</summary>
    Task IAsyncLifetime.InitializeAsync() => StartAsync().AsTask();

    /// <summary>Disposes the hosts, then the container.</summary>
    async Task IAsyncLifetime.DisposeAsync()
    {
        await DisposeAsync();
        if (Directory.Exists(_keyPath))
            Directory.Delete(_keyPath, recursive: true);
    }
}
