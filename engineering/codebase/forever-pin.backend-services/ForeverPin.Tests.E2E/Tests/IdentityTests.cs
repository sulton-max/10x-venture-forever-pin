using System.Net;
using AwesomeAssertions;
using ForeverPin.Tests.E2E.Harness;
using ForeverPin.Tests.E2E.Support;
using WoW.Two.Sdk.Backend.Beta.Testing.Web;

namespace ForeverPin.Tests.E2E.Tests;

/// <summary>E2E identity flow — anonymous, mint guest, then guest.</summary>
[Collection(AppCollection.Name)]
public sealed class IdentityTests(AppFixture fixture) : E2EBase(fixture)
{
    [Fact]
    public async Task Me_WithoutCookie_IsAnonymous()
    {
        var response = await AnonymousClient.GetAsync("/api/identity/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var me = await response.ReadEnvelopeAsync<MeResponseDto>();
        me.Kind.Should().Be("anonymous");
    }

    [Fact]
    public async Task Guest_SetsUserIdCookie()
    {
        var client = AnonymousClient;

        var response = await client.PostAsync("/api/identity/guest", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.TryGetValues("Set-Cookie", out var cookies).Should().BeTrue();
        cookies!.Should().Contain(c => c.StartsWith($"{AppFixture.UserIdCookieName}=", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Me_AfterGuestProvisioning_IsGuest()
    {
        var guest = await CreateGuestClientAsync();

        var response = await guest.Client.GetAsync("/api/identity/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var me = await response.ReadEnvelopeAsync<MeResponseDto>();
        me.Kind.Should().Be("guest");
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GetCode_ShouldReturn401_WhenGuestCapabilityIsForged(bool tampered)
    {
        var owner = await CreateGuestClientAsync();
        var code = await (await owner.Client.PostJsonAsync("/api/codes",
            CodeRequests.StaticUrl("Private", "https://example.com/private")))
            .ReadEnvelopeAsync<CodeDtoModel>();
        using var attacker = Fixture.CreateApiClient();
        var cookie = tampered
            ? owner.Cookie[..20] + (owner.Cookie[20] == 'A' ? 'B' : 'A') + owner.Cookie[21..]
            : owner.UserId.ToString();
        attacker.DefaultRequestHeaders.Add("Cookie", $"{AppFixture.UserIdCookieName}={cookie}");

        var me = await (await attacker.GetAsync("/api/identity/me")).ReadEnvelopeAsync<MeResponseDto>();
        me.Kind.Should().Be("anonymous");
        (await attacker.GetAsync($"/api/codes/{code.Id}")).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        (await owner.Client.GetAsync($"/api/codes/{code.Id}")).StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
