using System.Net;
using System.Text.Json;
using AwesomeAssertions;
using ForeverPin.Tests.E2E.Harness;

namespace ForeverPin.Tests.E2E.Tests;

/// <summary>E2E system status — each host reports its name and running version anonymously.</summary>
[Collection(AppCollection.Name)]
public sealed class SystemStatusTests(AppFixture fixture) : E2EBase(fixture)
{
    [Fact]
    public async Task Status_ShouldReturnServiceAndVersion_OnTheManagementHost()
    {
        using var response = await AnonymousClient.GetAsync("/api/system/status");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var status = body.RootElement.GetProperty("data");
        status.GetProperty("service").GetString().Should().Be("forever-pin-api");
        status.GetProperty("status").GetString().Should().Be("ok");
        status.GetProperty("version").GetString().Should().NotBeNullOrWhiteSpace().And.NotBe("unknown");
    }

    [Fact]
    public async Task Status_ShouldReturnServiceAndVersion_OnTheRedirectHost()
    {
        using var response = await RedirectClient.GetAsync("/api/system/status");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        body.RootElement.GetProperty("service").GetString().Should().Be("forever-pin-redirect");
        body.RootElement.GetProperty("status").GetString().Should().Be("ok");
        body.RootElement.GetProperty("version").GetString().Should().NotBeNullOrWhiteSpace();
    }
}
