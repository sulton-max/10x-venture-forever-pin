using System.Net;
using AwesomeAssertions;
using ForeverPin.Application.Billing.Core.Commands;
using ForeverPin.Application.Billing.Core.Models;
using ForeverPin.Application.Settings;
using ForeverPin.Tests.E2E.Harness;
using ForeverPin.Tests.E2E.Support;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using WoW.Two.Sdk.Backend.Beta.Foundation.Errors;
using WoW.Two.Sdk.Backend.Beta.Mediator;
using WoW.Two.Sdk.Backend.Beta.Mediator.Result;
using WoW.Two.Sdk.Backend.Beta.Testing.Web;

namespace ForeverPin.Tests.E2E.Tests;

/// <summary>Verifies product host configuration, identity persistence, and pipeline wiring.</summary>
[Collection(AppCollection.Name)]
public sealed class HostConfigurationTests(AppFixture fixture) : E2EBase(fixture)
{
    [Fact]
    public void Configuration_ShouldUseHostOverrides_WhenBothHostsShareTheFixture()
    {
        Fixture.ApiHost.Services.GetRequiredService<IHostEnvironment>().EnvironmentName.Should().Be(Environments.Development);
        Fixture.RedirectHost.Services.GetRequiredService<IHostEnvironment>().EnvironmentName.Should().Be(Environments.Development);
        Fixture.ApiHost.Services.GetRequiredService<ApiSettings>().RedirectBaseUrl.Should().Be(AppFixture.RedirectBaseUrl);
        var management = Fixture.ApiHost.Services.GetRequiredService<IConfiguration>();
        var redirect = Fixture.RedirectHost.Services.GetRequiredService<IConfiguration>();
        management["DatabaseSettings:ConnectionString"].Should().Be(Fixture.Postgres.ConnectionString);
        redirect["DatabaseSettings:ConnectionString"].Should().Be(Fixture.Postgres.ConnectionString);
        Fixture.ApiHost.Services.GetRequiredService<IOptions<DataProtectionOptions>>()
            .Value.ApplicationDiscriminator.Should().Be("ForeverPin");
    }

    [Fact]
    public async Task Guest_ShouldRetainOwnership_WhenHostRestartsWithPersistedKeys()
    {
        string cookie;
        Guid codeId;
        using (var originalHost = Fixture.ApiHost.WithWebHostBuilder(_ => { }))
        {
            using var anonymous = originalHost.CreateSpaClient(AppFixture.TokenPath);
            using var provisioned = await anonymous.PostAsync("/api/identity/guest", content: null);
            provisioned.EnsureSuccessStatusCode();
            cookie = AppFixture.ExtractCookie(provisioned, AppFixture.UserIdCookieName)!;
            Guid.TryParse(cookie, out _).Should().BeFalse();
            using var owner = originalHost.CreateSpaClient(AppFixture.TokenPath);
            owner.DefaultRequestHeaders.Add("Cookie", $"{AppFixture.UserIdCookieName}={cookie}");
            codeId = (await (await owner.PostJsonAsync("/api/codes",
                CodeRequests.StaticUrl("Survives restart", "https://example.com/restart")))
                .ReadEnvelopeAsync<CodeDtoModel>()).Id;
        }

        using var restartedHost = Fixture.ApiHost.WithWebHostBuilder(_ => { });
        using var restored = restartedHost.CreateSpaClient(AppFixture.TokenPath);
        restored.DefaultRequestHeaders.Add("Cookie", $"{AppFixture.UserIdCookieName}={cookie}");
        var response = await restored.GetAsync($"/api/codes/{codeId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        (await response.ReadEnvelopeAsync<CodeDtoModel>()).Id.Should().Be(codeId);
        var keyPath = restartedHost.Services.GetRequiredService<IConfiguration>()["DataProtection:KeyPath"]!;
        Directory.GetFiles(keyPath, "key-*.xml").Should().NotBeEmpty();
    }

    [Fact]
    public async Task Antiforgery_ShouldRejectACookieWrite_WithoutTheEchoedToken()
    {
        var owner = await CreateGuestClientAsync();
        using var browserlessClient = Fixture.ApiHost.CreateClient();
        browserlessClient.DefaultRequestHeaders.Add("Cookie", $"{AppFixture.UserIdCookieName}={owner.Cookie}");

        var rejected = await browserlessClient.PostJsonAsync("/api/codes",
            CodeRequests.StaticUrl("Forged", "https://example.com/forged"));
        var accepted = await owner.Client.PostJsonAsync("/api/codes",
            CodeRequests.StaticUrl("Echoed", "https://example.com/echoed"));

        rejected.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await rejected.Content.ReadAsStringAsync()).Should().Contain("AntiforgeryValidationFailed");
        accepted.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Mediator_ShouldReturnCanceledWithoutWriting_WhenWebhookRequestIsCanceled()
    {
        Fixture.Gateway.NextEvent = new BillingWebhookEvent
        {
            Type = BillingWebhookEventType.CheckoutSessionCompleted,
            EventId = FakeBillingBroker.NewEventId(),
            CreatedAt = FakeBillingBroker.EventTime,
                        UserId = Guid.NewGuid(),
            StripeCustomerId = "cus-canceled",
            StripeSubscriptionId = "sub-canceled",
            PriceId = AppFixture.PricePro,
        };
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();
        using var scope = Fixture.ApiHost.Services.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        var result = await sender.SendAsync(new BillingWebhookCommand
        {
            RawBody = "{}",
            StripeSignature = "test-signature",
        }, cancellation.Token);

        result.Should().BeOfType<AppResult<BillingWebhookResult.Success>.Failure>()
            .Subject.Error.Type.Should().Be(AppErrorType.Canceled);
        await using var context = Fixture.NewDbContext();
        (await context.Subscriptions.CountAsync()).Should().Be(0);
    }
}
