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
    private readonly RoutingService _routingService = new();

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

    private static ScanContext Context(DeviceType device) => new()
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
}
