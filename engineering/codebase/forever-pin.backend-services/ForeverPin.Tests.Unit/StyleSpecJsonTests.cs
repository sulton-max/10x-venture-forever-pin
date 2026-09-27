using System.Text.Json;
using ForeverPin.Domain.Codes.Core.Entities;
using ForeverPin.Domain.Codes.Serialization.Factories;
using ForeverPin.Infrastructure.Codes.Core.Extensions;
using WoW.Two.Sdk.Backend.Beta.Codes.Models;
using WoW.Two.Sdk.Backend.Beta.Codes.Models.Style;

namespace ForeverPin.Tests.Unit;

/// <summary>Proves the <see cref="StyleSpec"/> ↔ <c>StyleJson</c> round-trip and the forgiving fallback.</summary>
public class StyleSpecJsonTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("{}")]
    [InlineData("not json")]
    public void Deserialize_blank_or_malformed_falls_back_to_default(string? json)
    {
        var spec = ReadStoredStyle(json);

        Assert.Equal(StyleSpec.Default, spec);
    }

    [Fact]
    public void Round_trips_a_styled_spec_through_camelcase_json()
    {
        var original = StyleSpec.Default with
        {
            ForegroundColor = "#112233",
            BackgroundColor = "#445566",
            TransparentBackground = true,
            EccLevel = EccLevel.H,
            QuietZoneModules = 6,
            Logo = new LogoSpec { DataUrl = "data:image/png;base64,AAAA", SizeRatio = 0.3 },
        };

        var json = JsonSerializer.Serialize(original, CodeJsonOptionsFactory.CreateStyle());
        var restored = ReadStoredStyle(json);

        Assert.Equal(original, restored);
        Assert.Contains("foregroundColor", json);        // camelCase
        Assert.Contains("\"eccLevel\":\"H\"", json);      // string enum
    }
    private static StyleSpec ReadStoredStyle(string? json) => new CodeEntity
    {
        Id = Guid.NewGuid(), UserId = Guid.NewGuid(), Name = "Style corpus", StyleJson = json!,
    }.ToModel("https://redirect.test").Style;
}
