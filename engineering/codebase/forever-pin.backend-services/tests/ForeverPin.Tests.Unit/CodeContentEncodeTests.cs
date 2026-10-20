using ForeverPin.Common.Domain.Codes.Content.MobileApp.Enums;
using ForeverPin.Common.Domain.Codes.Content.Wifi.Enums;
using ForeverPin.Common.Domain.Codes.Core.Enums;
using ForeverPin.Domain.Codes.Content;
using ForeverPin.Domain.Codes.Content.Calendar.Models;
using ForeverPin.Domain.Codes.Content.Email.Models;
using ForeverPin.Domain.Codes.Content.Geo.Models;
using ForeverPin.Domain.Codes.Content.MobileApp.Models;
using ForeverPin.Domain.Codes.Content.Phone.Models;
using ForeverPin.Domain.Codes.Content.Sms.Models;
using ForeverPin.Domain.Codes.Content.Text.Models;
using ForeverPin.Domain.Codes.Content.Url.Models;
using ForeverPin.Domain.Codes.Content.VCard.Models;
using ForeverPin.Domain.Codes.Content.Wifi.Models;
using ForeverPin.Domain.Codes.Rules.Mappers;
using ForeverPin.Domain.Codes.Rules.Models;
using WoW.Two.Sdk.Beta.Be.Codes.Payloads.Exporters;
using WoW.Two.Sdk.Beta.Be.Codes.Payloads.Serializers;
using WoW.Two.Sdk.Beta.Be.Foundation.Results;

namespace ForeverPin.Tests.Unit;

/// <summary>Proves product-to-SDK mapping preserves fields and selects the correct mode payload.</summary>
public sealed class CodeContentEncodeTests
{
    private readonly CodePayloadMapper _mapper = new(new CodePayloadSerializer(), new VCardExporter(), new FloatingCalendarEventExporter());

    public static TheoryData<CodeContentValueObject, string> Payloads() => new()
    {
        { new UrlContentValueObject { Url = "https://example.com/a" }, "https://example.com/a" },
        { new MobileAppLinkContentValueObject { Store = MobileAppStoreType.AppStore, Url = "https://apps.apple.com/a" }, "https://apps.apple.com/a" },
        { new TextContentValueObject { Text = " text " }, " text " },
        { new PhoneContentValueObject { Phone = "+15550100" }, "tel:+15550100" },
        { new SmsContentValueObject { Phone = "+15550100", Message = "hey" }, "SMSTO:+15550100:hey" },
        { new EmailContentValueObject { To = "a+b@c.com", Subject = "Hi & bye", Body = "line one\n+é" }, "mailto:a%2Bb@c.com?subject=Hi%20%26%20bye&body=line%20one%0D%0A%2B%C3%A9" },
        { new GeoContentValueObject { Latitude = 41.31, Longitude = 69.24 }, "geo:41.31,69.24" },
        { new WifiContentValueObject { Ssid = " Cafe ", Password = " x ", Encryption = WifiEncryption.Wpa, Hidden = true }, "WIFI:T:WPA;S: Cafe ;P: x ;H:true;;" },
        { new WifiContentValueObject { Ssid = "Open", Password = "ignored", Encryption = WifiEncryption.None }, "WIFI:T:nopass;S:Open;;" },
        { new WifiContentValueObject { Ssid = "Net", Password = "pw", Encryption = WifiEncryption.Wep }, "WIFI:T:WEP;S:Net;P:pw;;" },
    };

    [Theory]
    [MemberData(nameof(Payloads))]
    public void Maps_each_product_payload(CodeContentValueObject content, string expected) =>
        Assert.Equal(expected, _mapper.Map(content).ValueOrThrow());

    [Fact]
    public void Static_urls_bake_the_destination_and_dynamic_urls_bake_the_short_link()
    {
        CodeRuleValueObject[] rules = [new DefaultRuleValueObject { Content = new UrlContentValueObject { Url = "https://destination.test" } }];
        Assert.Equal("https://destination.test", _mapper.Resolve(ContentMode.Static, rules, "https://short.test/a").ValueOrThrow());
        Assert.Equal("https://short.test/a", _mapper.Resolve(ContentMode.Dynamic, rules, "https://short.test/a").ValueOrThrow());
        Assert.False(_mapper.Resolve(ContentMode.Static, [], "https://short.test/a").IsSuccess);
    }

    [Theory]
    [InlineData("javascript:alert(1)")]
    [InlineData("/relative")]
    [InlineData("https://user:password@example.com")]
    [InlineData("https://example.com\r\nLocation:evil")]
    public void Unsafe_url_and_mobile_destinations_fail(string url)
    {
        Assert.False(_mapper.Map(new UrlContentValueObject { Url = url }).IsSuccess);
        Assert.False(_mapper.Map(new MobileAppLinkContentValueObject { Store = MobileAppStoreType.AppStore, Url = url }).IsSuccess);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Empty_text_fails_instead_of_baking_an_empty_symbol(string? text) =>
        Assert.False(_mapper.Map(new TextContentValueObject { Text = text! }).IsSuccess);

    [Fact]
    public void Contact_maps_organization_and_optional_fields_to_the_exporter()
    {
        var output = _mapper.Map(new VCardContentValueObject
        {
            FirstName = "Ada", LastName = "Lovelace", Org = "Babbage, Inc", Phone = "+15550100", Note = "hello\rINJECT:bad",
        }).ValueOrThrow();
        Assert.Contains("N:Lovelace;Ada;;;\r\nFN:Ada Lovelace", output);
        Assert.Contains("ORG:Babbage\\, Inc", output);
        Assert.Contains("NOTE:hello\\nINJECT:bad", output);
        Assert.DoesNotContain("EMAIL:", output);
    }

    [Fact]
    public void Calendar_preserves_floating_fields_and_optional_end()
    {
        var content = new CalendarContentValueObject { Title = "Launch", Start = new DateTime(2026, 7, 1, 18, 30, 0), Location = "HQ" };
        var output = _mapper.Map(content).ValueOrThrow();
        Assert.Contains("DTSTART:20260701T183000\r\nLOCATION:HQ", output);
        Assert.DoesNotContain("DTEND", output);
        Assert.Contains("DTEND:20260701T190000", _mapper.Map(content with { End = new DateTime(2026, 7, 1, 19, 0, 0) }).ValueOrThrow());
        Assert.False(_mapper.Map(content with { End = content.Start }).IsSuccess);
    }
}
