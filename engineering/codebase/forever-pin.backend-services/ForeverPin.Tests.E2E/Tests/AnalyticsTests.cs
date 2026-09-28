using System.Net;
using AwesomeAssertions;
using ForeverPin.Domain.Codes.Core.Entities;
using ForeverPin.Domain.Codes.Core.Enums;
using ForeverPin.Tests.E2E.Harness;
using ForeverPin.Tests.E2E.Support;
using Microsoft.EntityFrameworkCore;
using WoW.Two.Sdk.Backend.Beta.Testing.Web;

namespace ForeverPin.Tests.E2E.Tests;

/// <summary>E2E analytics — the owner reads a code's recorded scans per UTC day and per attribute.</summary>
[Collection(AppCollection.Name)]
public sealed class AnalyticsTests(AppFixture fixture) : E2EBase(fixture)
{
    [Fact]
    public async Task Analytics_ShouldGroupTheWindowByDayDeviceSystemAndCountry()
    {
        var owner = await CreateGuestClientAsync();
        var code = await CreateCodeAsync(owner.Client);
        var now = Fixture.ApiHost.Clock.GetUtcNow();
        await SeedScansAsync(code.Id,
            (now, DeviceType.Ios, "iOS", "AU"),
            (now, DeviceType.Android, "Android", null),
            (now.AddDays(-1), DeviceType.Ios, "iOS", "AU"),
            (now.AddDays(-7), DeviceType.Desktop, "Windows", "US"));

        var response = await owner.Client.GetAsync($"/api/codes/{code.Id}/analytics?days=7");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var analytics = await response.ReadEnvelopeAsync<CodeAnalyticsDtoModel>();
        var today = DateOnly.FromDateTime(now.UtcDateTime);
        analytics.To.Should().Be(today);
        analytics.From.Should().Be(today.AddDays(-6));
        analytics.TotalScans.Should().Be(4);
        analytics.RangeScans.Should().Be(3);
        analytics.Daily.Select(day => day.Scans).Should().Equal(0, 0, 0, 0, 0, 1, 2);
        analytics.Devices.Should().Equal(new ScanShareDtoModel("ios", 2), new ScanShareDtoModel("android", 1));
        analytics.OperatingSystems.Should().Equal(new ScanShareDtoModel("iOS", 2), new ScanShareDtoModel("Android", 1));
        analytics.Countries.Should().Equal(new ScanShareDtoModel("AU", 2), new ScanShareDtoModel(null, 1));
    }

    [Fact]
    public async Task Analytics_ShouldReturnAnEmptyWindow_WhenTheCodeWasNeverScanned()
    {
        var owner = await CreateGuestClientAsync();
        var code = await CreateCodeAsync(owner.Client);

        var analytics = await (await owner.Client.GetAsync($"/api/codes/{code.Id}/analytics"))
            .ReadEnvelopeAsync<CodeAnalyticsDtoModel>();

        analytics.Daily.Should().HaveCount(30).And.OnlyContain(day => day.Scans == 0);
        analytics.RangeScans.Should().Be(0);
        analytics.Devices.Should().BeEmpty();
    }

    [Fact]
    public async Task Analytics_ShouldHideTheCode_FromAnyoneButItsOwner()
    {
        var owner = await CreateGuestClientAsync();
        var stranger = await CreateGuestClientAsync();
        var code = await CreateCodeAsync(owner.Client);

        var strangerRead = await stranger.Client.GetAsync($"/api/codes/{code.Id}/analytics");
        var anonymousRead = await AnonymousClient.GetAsync($"/api/codes/{code.Id}/analytics");

        strangerRead.StatusCode.Should().Be(HttpStatusCode.NotFound);
        anonymousRead.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(367)]
    public async Task Analytics_ShouldReturn400_WhenTheWindowIsOutOfRange(int days)
    {
        var owner = await CreateGuestClientAsync();
        var code = await CreateCodeAsync(owner.Client);

        var response = await owner.Client.GetAsync($"/api/codes/{code.Id}/analytics?days={days}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private static async Task<CodeDtoModel> CreateCodeAsync(HttpClient client) =>
        await (await client.PostJsonAsync("/api/codes", CodeRequests.DynamicUrl("Menu", "https://example.com/menu")))
            .ReadEnvelopeAsync<CodeDtoModel>();

    // Scans are seeded, not redirected: each host keeps its own fake clock, and the window reads the Api's.
    private async Task SeedScansAsync(
        Guid codeId, params (DateTimeOffset At, DeviceType Device, string Os, string? Country)[] scans)
    {
        await using var context = Fixture.NewDbContext();
        context.ScanEvents.AddRange(scans.Select(scan => new ScanEventEntity
        {
            Id = Guid.NewGuid(),
            CodeId = codeId,
            ScannedAt = scan.At,
            Device = scan.Device,
            Os = scan.Os,
            CountryCode = scan.Country,
            DestinationUrl = "https://example.com/menu",
        }));
        await context.SaveChangesAsync();
        await context.Codes.Where(code => code.Id == codeId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(code => code.ScanCount, scans.Length));
    }
}
