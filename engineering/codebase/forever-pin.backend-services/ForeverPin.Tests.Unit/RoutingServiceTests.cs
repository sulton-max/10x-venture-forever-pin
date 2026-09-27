using WoW.Two.Sdk.Backend.Beta.Web.RequestContext.Parsers;
using WoW.Two.Sdk.Backend.Beta.Foundation.Results;
using ForeverPin.Domain.Codes.Rules.Mappers;
using ForeverPin.Common.Domain.Codes.Content.MobileApp.Enums;
using ForeverPin.Domain.Codes.Content.MobileApp.Models;
using ForeverPin.Domain.Codes.Content.Text.Models;
using WoW.Two.Sdk.Backend.Beta.Codes.Payloads.Exporters;
using WoW.Two.Sdk.Backend.Beta.Codes.Payloads.Serializers;
using ForeverPin.Common.Domain.Codes.Core.Enums;
using ForeverPin.Domain.Codes.Content.Phone.Models;
using ForeverPin.Domain.Codes.Content.Url.Models;
using ForeverPin.Domain.Codes.Core.Entities;
using ForeverPin.Domain.Codes.Core.Enums;
using ForeverPin.Domain.Codes.Rules.Models;
using ForeverPin.Redirect.Api.Application.Routing.Models;
using ForeverPin.Redirect.Api.Infrastructure.Routing;

namespace ForeverPin.Tests.Unit;

/// <summary>Proves the routing engine — first-match-wins, the optional catch-all, and active gating.</summary>
/// <remarks>Carry <see cref="PhoneContentValueObject"/> in a rule to exercise a Redirect.</remarks>
public class RoutingServiceTests
{
    private readonly RoutingService _routingService = new(new CodePayloadMapper(new CodePayloadSerializer(), new VCardExporter(), new FloatingCalendarEventExporter()));

    [Theory]
    [InlineData("https://example.com/path", true)]
    [InlineData("http://example.com/path", true)]
    [InlineData("javascript:alert(1)", false)]
    [InlineData("/relative", false)]
    [InlineData("", false)]
    public void Dynamic_url_resolves_only_absolute_http_destinations(string destination, bool redirects)
    {
        var code = Code(new DefaultRuleValueObject { Content = new UrlContentValueObject { Url = destination } });
        var result = _routingService.Evaluate(code, Context(DeviceType.Desktop));
        if (redirects)
            Assert.Equal(destination, Assert.IsType<RoutingResult.Redirect>(result).Destination);
        else
            Assert.IsType<RoutingResult.NotFound>(result);
    }

    private static CodeEntity Code(params CodeRuleValueObject[] rules) => new()
    {
        Id = Guid.NewGuid(),
        Slug = "abc1234",
        UserId = Guid.NewGuid(),
        Name = "Test",
        BarcodeFormat = BarcodeFormat.QrCode,
        StyleJson = "{}",
        Mode = ContentMode.Dynamic,
        ContentType = CodeContentType.Phone,
        IsActive = true,
        Rules = [.. rules],
    };

    private static ScanContextModel Context(DeviceType device) => new()
    {
        Slug = "abc1234",
        Device = device,
        NowUtc = DateTimeOffset.UnixEpoch,
    };

    private static ConditionalRuleValueObject DeviceRule(int order, string value, string destination) => new()
    {
        Order = order,
        Condition = RuleConditionType.Device,
        ConditionValue = value,
        Content = new PhoneContentValueObject { Phone = destination },
    };

    // The catch-all is a rule role, not a condition — it carries its own content and is never order-matched.
    private static DefaultRuleValueObject Fallback(string destination) => new()
    {
        Content = new PhoneContentValueObject { Phone = destination },
    };

    [Fact]
    public void First_matching_device_rule_wins()
    {
        var code = Code(
            DeviceRule(1, "Ios", "+1000000001"),
            DeviceRule(2, "Android", "+1000000002"));

        var result = _routingService.Evaluate(code, Context(DeviceType.Ios));

        var redirect = Assert.IsType<RoutingResult.Redirect>(result);
        Assert.Equal("tel:+1000000001", redirect.Destination);
        Assert.Equal(1, redirect.MatchedRuleOrder);
    }

    [Fact]
    public void No_matching_rule_and_no_default_is_not_found()
    {
        var code = Code(DeviceRule(1, "Ios", "+1000000001"));

        var result = _routingService.Evaluate(code, Context(DeviceType.Desktop));

        Assert.IsType<RoutingResult.NotFound>(result);
    }

    [Fact]
    public void Default_rule_catches_all_when_no_specific_rule_matches()
    {
        var code = Code(
            DeviceRule(1, "Ios", "+1000000001"),
            Fallback("+1000000009"));

        var result = _routingService.Evaluate(code, Context(DeviceType.Desktop));

        var redirect = Assert.IsType<RoutingResult.Redirect>(result);
        Assert.Equal("tel:+1000000009", redirect.Destination);
        Assert.Null(redirect.MatchedRuleOrder); // the catch-all is not an order-matched conditional rule
    }

    [Fact]
    public void Inactive_code_is_not_found()
    {
        var code = Code() with { IsActive = false };

        var result = _routingService.Evaluate(code, Context(DeviceType.Ios));

        Assert.IsType<RoutingResult.NotFound>(result);
    }

    [Theory]
    [InlineData("https://apps.apple.com/a", true)]
    [InlineData("javascript:alert(1)", false)]
    [InlineData("https://user:password@example.com", false)]
    public void Mobile_app_rules_use_validated_store_destinations(string url, bool redirects)
    {
        var code = Code(new DefaultRuleValueObject { Content = new MobileAppLinkContentValueObject { Store = MobileAppStoreType.AppStore, Url = url } });
        var result = _routingService.Evaluate(code, Context(DeviceType.Ios));
        if (redirects) Assert.Equal(url, Assert.IsType<RoutingResult.Redirect>(result).Destination);
        else Assert.IsType<RoutingResult.NotFound>(result);
    }

    [Theory]
    [InlineData("javascript:alert(1)")]
    [InlineData("/relative")]
    [InlineData("ordinary text")]
    [InlineData("https://example.com/text-is-not-a-url-rule")]
    public void Text_documents_are_not_interpreted_as_redirect_locations(string text)
    {
        var code = Code(new DefaultRuleValueObject { Content = new TextContentValueObject { Text = text } });
        Assert.IsType<RoutingResult.NotFound>(_routingService.Evaluate(code, Context(DeviceType.Desktop)));
    }

    [Theory]
    [InlineData("2026-01-01T23:45:00Z", true)]
    [InlineData("2026-01-02T00:30:00Z", true)]
    [InlineData("2026-01-02T01:00:00Z", false)]
    public void Utc_time_windows_wrap_midnight_and_exclude_the_end(string instant, bool matches)
    {
        var rule = DeviceRule(1, "23:30-01:00", "+1000000001") with { Condition = RuleConditionType.TimeOfDay };
        var result = _routingService.Evaluate(Code(rule), Context(DeviceType.Desktop) with { NowUtc = DateTimeOffset.Parse(instant) });
        Assert.Equal(matches, result is RoutingResult.Redirect);
    }

    [Theory]
    [InlineData("en", "en-US;q=0.8,ru;q=0.2", true)]
    [InlineData("en-US", "en;q=1,en-US;q=0", false)]
    [InlineData("en-US", "en;q=0,en-US;q=0.8", true)]
    [InlineData("en", "en;q=0,*;q=1", false)]
    [InlineData("en", "*;q=1", false)]
    [InlineData("ru", "en;q=0.8,ru;q=0.2", true)]
    public void Language_preferences_preserve_specific_exclusions_and_ignore_wildcard_only_matching(string language, string header, bool matches)
    {
        var rule = DeviceRule(1, language, "+1000000001") with { Condition = RuleConditionType.Language };
        var context = Context(DeviceType.Desktop) with { LanguagePreferences = new AcceptLanguageParser().Parse(header).ValueOrThrow() };
        Assert.Equal(matches, _routingService.Evaluate(Code(rule), context) is RoutingResult.Redirect);
    }
}
