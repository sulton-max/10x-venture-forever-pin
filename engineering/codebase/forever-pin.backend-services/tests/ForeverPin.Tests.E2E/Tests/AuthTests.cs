using System.Net;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using ForeverPin.Tests.E2E.Harness;
using ForeverPin.Tests.E2E.Support;
using WoW.Two.Sdk.Beta.Be.Testing.Web;

namespace ForeverPin.Tests.E2E.Tests;

/// <summary>E2E auth — Google sign-in, guest-code claim, cross-device ownership, sign-out.</summary>
[Collection(AppCollection.Name)]
public sealed class AuthTests(AppFixture fixture) : E2EBase(fixture)
{
    private static string Token(string subject, string email, string name) => $"fake:{subject}:{email}:{name}";

    [Fact]
    public async Task SignIn_NewAccount_ReturnsUserAndSetsAuthCookie()
    {
        var response = await AnonymousClient.PostJsonAsync(
            "/api/identity/sign-in", new { idToken = Token("sub-new", "alice@example.com", "Alice") });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var me = await response.ReadEnvelopeAsync<MeResponseDto>();
        me.Kind.Should().Be("user");

        response.Headers.TryGetValues("Set-Cookie", out var cookies).Should().BeTrue();
        cookies!.Should().Contain(c => c.StartsWith($"{AppFixture.AuthCookieName}=", StringComparison.Ordinal));
    }

    [Fact]
    public async Task SignIn_InvalidToken_Returns401()
    {
        var response = await AnonymousClient.PostJsonAsync("/api/identity/sign-in", new { idToken = "not-a-valid-token" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Me_AfterSignIn_IsUserWithProfile()
    {
        var (_, authed) = await SignInAsync(Token("sub-bob", "bob@example.com", "Bob"));

        var response = await authed.GetAsync("/api/identity/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var me = await response.ReadEnvelopeAsync<MeWithUserDto>();
        me.Kind.Should().Be("user");
        me.User!.Email.Should().Be("bob@example.com");
        me.User.Name.Should().Be("Bob");
    }

    [Fact]
    public async Task SignIn_ClaimsGuestCodes_SameDevice()
    {
        // A guest creates a code...
        var guest = await CreateGuestClientAsync();
        var created = await (await guest.Client.PostJsonAsync(
                "/api/codes", CodeRequests.StaticUrl("Guest menu", "https://example.com/menu")))
            .ReadEnvelopeAsync<CodeDtoModel>();

        // ...then signs in on the SAME device (the guest cookie rides along on the request).
        var (response, authed) = await SignInAsync(Token("sub-carol", "carol@example.com", "Carol"), guest.Client);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // The account now owns the code the guest made.
        var codes = await (await authed.GetAsync("/api/codes")).ReadEnvelopeAsync<IReadOnlyList<CodeDtoModel>>();
        codes.Should().ContainSingle(c => c.Id == created.Id);

        var account = await (await authed.GetAsync("/api/identity/me")).ReadEnvelopeAsync<MeWithUserDto>();
        account.User!.Id.Should().NotBe(guest.UserId);
        (await guest.Client.GetAsync($"/api/codes/{created.Id}")).StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SignIn_ShouldLeaveVictimCodesOwned_WhenGuestCookieIsForged(bool tampered)
    {
        var victim = await CreateGuestClientAsync();
        var code = await (await victim.Client.PostJsonAsync("/api/codes",
            CodeRequests.StaticUrl("Victim", "https://example.com/private")))
            .ReadEnvelopeAsync<CodeDtoModel>();
        using var attacker = Fixture.CreateApiClient();
        var cookie = tampered
            ? victim.Cookie[..20] + (victim.Cookie[20] == 'A' ? 'B' : 'A') + victim.Cookie[21..]
            : victim.UserId.ToString();
        attacker.DefaultRequestHeaders.Add("Cookie", $"{AppFixture.UserIdCookieName}={cookie}");

        var (response, signedIn) = await SignInAsync(Token("attacker", "attacker@example.com", "Attacker"), attacker);
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var codes = await (await signedIn.GetAsync("/api/codes")).ReadEnvelopeAsync<IReadOnlyList<CodeDtoModel>>();
        codes.Should().BeEmpty();
        (await victim.Client.GetAsync($"/api/codes/{code.Id}")).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SignIn_ShouldCreateOneAccount_WhenSubjectSignsInConcurrently()
    {
        var responses = await Task.WhenAll(Enumerable.Range(0, 8).Select(_ =>
            SignInAsync(Token("concurrent-subject", "same@example.com", "Same"))));

        foreach (var (response, _) in responses)
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        var accounts = await Task.WhenAll(responses.Select(async result =>
            await (await result.Authed.GetAsync("/api/identity/me")).ReadEnvelopeAsync<MeWithUserDto>()));
        accounts.Select(account => account.User!.Id).Distinct().Should().ContainSingle();
        await using var context = Fixture.NewDbContext();
        (await context.Users.CountAsync(user => user.GoogleSubject == "concurrent-subject")).Should().Be(1);
    }

    [Fact]
    public async Task SignIn_ShouldClaimWholeGuestOwnership_WhenDifferentAccountsCompete()
    {
        var guest = await CreateGuestClientAsync();
        var first = await (await guest.Client.PostJsonAsync("/api/codes",
            CodeRequests.StaticUrl("First", "https://example.com/first"))).ReadEnvelopeAsync<CodeDtoModel>();
        var second = await (await guest.Client.PostJsonAsync("/api/codes",
            CodeRequests.StaticUrl("Second", "https://example.com/second"))).ReadEnvelopeAsync<CodeDtoModel>();

        var claims = await Task.WhenAll(
            SignInAsync(Token("claim-a", "a@example.com", "A"), guest.Client),
            SignInAsync(Token("claim-b", "b@example.com", "B"), guest.Client));

        foreach (var (response, _) in claims)
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        var lists = await Task.WhenAll(claims.Select(async claim =>
            await (await claim.Authed.GetAsync("/api/codes")).ReadEnvelopeAsync<IReadOnlyList<CodeDtoModel>>()));
        lists.Select(list => list.Count).Should().BeEquivalentTo([0, 2]);
        lists.SelectMany(list => list).Select(code => code.Id).Should().BeEquivalentTo([first.Id, second.Id]);
        await using var context = Fixture.NewDbContext();
        (await context.Codes.Select(code => code.UserId).Distinct().ToListAsync()).Should().ContainSingle();
        (await context.Codes.AnyAsync(code => code.UserId == guest.UserId)).Should().BeFalse();
    }

    [Fact]
    public async Task SignIn_ShouldRefreshTheStoredProfile_WhenGoogleDetailsChange()
    {
        await SignInAsync(Token("sub-frank", "frank@example.com", "Frank"));

        var (response, _) = await SignInAsync(Token("sub-frank", "frank@new.example", "Frank Ocean"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        await using var context = Fixture.NewDbContext();
        var account = await context.Users.SingleAsync(user => user.GoogleSubject == "sub-frank");
        account.Email.Should().Be("frank@new.example");
        account.Name.Should().Be("Frank Ocean");
    }

    [Fact]
    public async Task SignedIn_CodesFollowAccount_AcrossDevices()
    {
        // Device A: sign in, create a code.
        var (_, deviceA) = await SignInAsync(Token("sub-dave", "dave@example.com", "Dave"));
        var created = await (await deviceA.PostJsonAsync(
                "/api/codes", CodeRequests.StaticUrl("Shop", "https://example.com/shop")))
            .ReadEnvelopeAsync<CodeDtoModel>();

        // Device B: a brand-new client signs in to the SAME account (no shared cookie) and sees the code.
        var (_, deviceB) = await SignInAsync(Token("sub-dave", "dave@example.com", "Dave"));
        var codes = await (await deviceB.GetAsync("/api/codes")).ReadEnvelopeAsync<IReadOnlyList<CodeDtoModel>>();

        codes.Should().Contain(c => c.Id == created.Id);
    }

    [Fact]
    public async Task Logout_ClearsTheSessionCookie()
    {
        var (_, authed) = await SignInAsync(Token("sub-erin", "erin@example.com", "Erin"));

        var logout = await authed.PostAsync("/api/identity/sign-out", content: null);

        logout.StatusCode.Should().Be(HttpStatusCode.NoContent);
        logout.Headers.TryGetValues("Set-Cookie", out var cookies).Should().BeTrue();
        // Sign-out deletes the cookie by re-issuing it empty (expired).
        cookies!.Should().Contain(c => c.StartsWith($"{AppFixture.AuthCookieName}=;", StringComparison.Ordinal));
    }

    /// <summary>Signs in with the fake Google token and returns a client carrying the session cookie.</summary>
    private async Task<(
        HttpResponseMessage Response,
        HttpClient Authed)> SignInAsync(string idToken, HttpClient? from = null)
    {
        var client = from ?? AnonymousClient;
        var response = await client.PostJsonAsync("/api/identity/sign-in", new { idToken });

        var authed = Fixture.CreateApiClient();
        if (AppFixture.ExtractCookie(response, AppFixture.AuthCookieName) is { } cookie)
            authed.DefaultRequestHeaders.Add("Cookie", $"{AppFixture.AuthCookieName}={cookie}");

        return (response, authed);
    }
}
