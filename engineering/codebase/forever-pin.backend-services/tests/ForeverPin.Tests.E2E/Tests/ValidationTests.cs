using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AwesomeAssertions;
using ForeverPin.Tests.E2E.Harness;
using ForeverPin.Tests.E2E.Support;
using WoW.Two.Sdk.Beta.Be.Testing.Web;

namespace ForeverPin.Tests.E2E.Tests;

/// <summary>Checks product validation paths through HTTP and the SDK error boundary.</summary>
[Collection(AppCollection.Name)]
public sealed class ValidationTests(AppFixture fixture) : E2EBase(fixture)
{
    [Fact]
    public async Task Create_ShouldReturn400WithNamePath_WhenNameIsBlank()
    {
        var owner = await CreateGuestClientAsync();
        var response = await owner.Client.PostJsonAsync("/api/codes", CodeRequests.StaticUrl("", "https://example.com"));

        var properties = await ReadValidationPropertiesAsync(response);
        properties.Should().Contain(property => property.Equals("Name", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Create_ShouldReturn400WithRulePath_WhenDeviceConditionIsInvalid()
    {
        var owner = await CreateGuestClientAsync();
        var response = await owner.Client.PostJsonAsync("/api/codes",
            CodeRequests.DynamicUrl("Invalid device", "https://example.com",
                [CodeRequests.ConditionalRule("Device", "unrecognized-device", new { type = "url", url = "https://example.com/ios" })]));

        var properties = await ReadValidationPropertiesAsync(response);
        properties.Should().Contain(property => property.Equals("Rules[0].ConditionValue", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Preview_ShouldReturn400WithStylePath_WhenForegroundContainsMarkup()
    {
        var response = await AnonymousClient.PostAsJsonAsync("/api/codes/preview", new
        {
            mode = "static",
            barcodeFormat = "QrCode",
            rules = new[] { CodeRequests.DefaultRule(new { type = "text", text = "preview" }) },
            style = new
            {
                foregroundColor = "<script>alert(1)</script>",
                backgroundColor = "#FFFFFF",
                transparentBackground = false,
                eccLevel = "Q",
                quietZoneModules = 4,
                moduleShape = "square",
                finderShape = "square",
                finderDotShape = "square",
            },
        });

        var properties = await ReadValidationPropertiesAsync(response);
        properties.Should().Contain(property => property.Equals("Style.ForegroundColor", StringComparison.OrdinalIgnoreCase));
        (await response.Content.ReadAsStringAsync()).Should().NotContain("alert(1)");
    }

    [Fact]
    public async Task Preview_ShouldReturn400_WhenRulesAreNull()
    {
        var response = await AnonymousClient.PostAsJsonAsync("/api/codes/preview", new
        {
            mode = "static",
            barcodeFormat = "QrCode",
            rules = (object?)null,
            style = CodeRequests.Style(),
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Preview_ShouldReturn400_WhenBarcodeEnumIsUndefined()
    {
        var response = await AnonymousClient.PostAsJsonAsync("/api/codes/preview", new
        {
            mode = "static",
            barcodeFormat = 999,
            rules = new[] { CodeRequests.DefaultRule(new { type = "text", text = "preview" }) },
            style = CodeRequests.Style(),
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Update_ShouldReturn400_WhenRulesConflictWithPersistedStaticMode()
    {
        var owner = await CreateGuestClientAsync();
        var code = await (await owner.Client.PostJsonAsync("/api/codes",
            CodeRequests.StaticUrl("Static", "https://example.com"))).ReadEnvelopeAsync<CodeDtoModel>();
        var response = await owner.Client.PutJsonAsync($"/api/codes/{code.Id}",
            CodeRequests.DynamicUrl("Still static", "https://example.com",
                [CodeRequests.IosRule("https://example.com/ios")]));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var saved = await (await owner.Client.GetAsync($"/api/codes/{code.Id}")).ReadEnvelopeAsync<CodeDtoModel>();
        saved.Mode.Should().Be("static");
        saved.Rules.Should().ContainSingle();
    }

    [Fact]
    public async Task Preview_ShouldReturnContentPath_WhenStaticPayloadExceedsRenderingLimit()
    {
        var response = await AnonymousClient.PostAsJsonAsync("/api/codes/preview", new
        {
            mode = "static", barcodeFormat = "QrCode",
            rules = new[] { CodeRequests.DefaultRule(new { type = "text", text = new string('x', 8193) }) },
            style = CodeRequests.Style(),
        });

        var properties = await ReadValidationPropertiesAsync(response);
        properties.Should().Contain("Rules[0].Content");
        properties.Should().NotContain("Payload");
    }

    [Fact]
    public async Task Preview_ShouldReturnOperationFailure_WhenGeneratedShortLinkCannotUseNumericBarcode()
    {
        var response = await AnonymousClient.PostAsJsonAsync("/api/codes/preview", new
        {
            mode = "dynamic", barcodeFormat = "Ean13",
            rules = new[] { CodeRequests.DefaultRule(new { type = "url", url = "https://example.com" }) },
            style = CodeRequests.Style(),
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        body.RootElement.TryGetProperty("errors", out _).Should().BeFalse();
    }

    [Fact]
    public async Task Update_ShouldResolveOwnershipBeforeValidatingNullGradientElements()
    {
        var owner = await CreateGuestClientAsync();
        var stranger = await CreateGuestClientAsync();
        var code = await (await owner.Client.PostJsonAsync("/api/codes",
            CodeRequests.StaticUrl("Owned", "https://example.com"))).ReadEnvelopeAsync<CodeDtoModel>();
        var request = new
        {
            name = "Updated", barcodeFormat = "QrCode", contentType = "url",
            rules = new[] { CodeRequests.DefaultRule(new { type = "url", url = "https://example.com" }) },
            style = new
            {
                foregroundColor = "#000000", backgroundColor = "#FFFFFF", transparentBackground = false,
                eccLevel = "Q", quietZoneModules = 4, moduleShape = "square", finderShape = "square", finderDotShape = "square",
                gradient = new { type = "linear", angle = 0, stops = new object?[] { null } },
            },
        };

        (await stranger.Client.PutJsonAsync($"/api/codes/{code.Id}", request)).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await owner.Client.PutJsonAsync($"/api/codes/{Guid.NewGuid()}", request)).StatusCode.Should().Be(HttpStatusCode.NotFound);
        var properties = await ReadValidationPropertiesAsync(await owner.Client.PutJsonAsync($"/api/codes/{code.Id}", request));
        properties.Should().Contain("Style.Gradient.Stops[0]");
        var unchanged = await (await owner.Client.GetAsync($"/api/codes/{code.Id}")).ReadEnvelopeAsync<CodeDtoModel>();
        unchanged.Name.Should().Be("Owned");
    }

    [Fact]
    public async Task Create_ShouldSucceedWithAWarning_WhenTheDestinationIsInsecure()
    {
        var owner = await CreateGuestClientAsync();
        var response = await owner.Client.PostJsonAsync("/api/codes",
            CodeRequests.DynamicUrl("Plain http", "http://menu.example.com/today"));

        var warnings = await ReadWarningsAsync(response);
        warnings.Should().ContainSingle();
        warnings[0].Code.Should().Be("RedirectTargetInsecure");
        warnings[0].Property.Should().Be("Rules[0].Content.Url");
        warnings[0].Severity.Should().BeEquivalentTo("warning");
    }

    [Fact]
    public async Task Update_ShouldSucceedWithAWarning_WhenTheDestinationIsOnAPrivateNetwork()
    {
        var owner = await CreateGuestClientAsync();
        var code = await (await owner.Client.PostJsonAsync("/api/codes",
            CodeRequests.DynamicUrl("Office wiki", "https://example.com"))).ReadEnvelopeAsync<CodeDtoModel>();

        var response = await owner.Client.PutJsonAsync($"/api/codes/{code.Id}",
            CodeRequests.DynamicUrl("Office wiki", "https://10.0.0.8/wiki"));

        (await ReadWarningsAsync(response)).Select(warning => warning.Code)
            .Should().Contain("RedirectTargetPrivateNetwork");
    }

    [Fact]
    public async Task Create_ShouldOmitWarnings_WhenTheDestinationIsPublicAndSecure()
    {
        var owner = await CreateGuestClientAsync();
        var response = await owner.Client.PostJsonAsync("/api/codes",
            CodeRequests.DynamicUrl("Menu", "https://example.com/menu"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        body.RootElement.TryGetProperty("warnings", out _).Should().BeFalse();
    }

    private static async Task<(string Property, string Code, string Severity)[]> ReadWarningsAsync(HttpResponseMessage response)
    {
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return body.RootElement.GetProperty("warnings").EnumerateArray()
            .Select(warning => (
                warning.GetProperty("property").GetString()!,
                warning.GetProperty("code").GetString()!,
                warning.GetProperty("severity").GetString()!))
            .ToArray();
    }

    private static async Task<string[]> ReadValidationPropertiesAsync(HttpResponseMessage response)
    {
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
        using var body = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        body.RootElement.GetProperty("status").GetInt32().Should().Be(400);
        return body.RootElement.GetProperty("errors").EnumerateArray()
            .Select(error => error.GetProperty("property").GetString()!).ToArray();
    }
}
