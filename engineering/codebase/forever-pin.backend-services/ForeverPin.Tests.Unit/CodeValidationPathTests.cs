using ForeverPin.Application.Codes.Core.Models;
using FluentValidation;
using ForeverPin.Application.Codes.Core.Commands;
using ForeverPin.Application.Codes.Core.Validators;
using ForeverPin.Application.Codes.Rules.Models;
using ForeverPin.Application.Codes.Rules.Validators;
using ForeverPin.Common.Domain.Codes.Core.Enums;
using ForeverPin.Domain.Codes.Content.Text.Models;
using ForeverPin.Domain.Codes.Content.Url.Models;
using ForeverPin.Domain.Codes.Core.Enums;
using ForeverPin.Domain.Codes.Rules.Models;
using WoW.Two.Sdk.Backend.Beta.Codes.Models.Style;

namespace ForeverPin.Tests.Unit;

/// <summary>Locks the property path every validation failure reports.</summary>
/// <remarks>Paths are asserted verbatim rather than by shape — that is the point of the test.</remarks>
public class CodeValidationPathTests
{
    private readonly CodeCreateCommandValidator _validator = new();

    private static CodeCreateCommand Command(ContentMode mode, params CodeRuleValueObject[] rules) => new()
    {
        UserId = Guid.NewGuid(),
        Name = "Test",
        ContentType = CodeContentType.Url,
        Mode = mode,
        Rules = rules,
        Style = StyleSpec.Default,
    };

    private static ConditionalRuleValueObject Conditional(int order, string url) => new()
    {
        Order = order,
        Condition = RuleConditionType.Device,
        ConditionValue = "Ios",
        Content = new UrlContentValueObject { Url = url },
    };

    private static DefaultRuleValueObject Fallback(string url) =>
        new() { Content = new UrlContentValueObject { Url = url } };

    private string[] PathsFor(CodeCreateCommand command) =>
        [.. _validator.Validate(command).Errors.Select(failure => failure.PropertyName)];

    [Fact]
    public void Content_failure_reports_the_rule_index_and_the_content_member()
    {
        var paths = PathsFor(Command(ContentMode.Static, Fallback(string.Empty)));

        Assert.Contains("Rules[0].Content.Url", paths);
    }

    [Fact]
    public void Condition_failure_reports_the_rule_index_and_the_condition_member()
    {
        var rule = Conditional(1, "https://a.io") with { ConditionValue = string.Empty };
        var paths = PathsFor(Command(ContentMode.Dynamic, rule, Fallback("https://b.io")));

        Assert.Contains("Rules[0].ConditionValue", paths);
    }

    [Fact]
    public void Homogeneity_failure_reports_ContentType_not_Rules()
    {
        var rule = new DefaultRuleValueObject { Content = new TextContentValueObject { Text = "hi" } };
        var paths = PathsFor(Command(ContentMode.Static, rule));

        Assert.Contains("ContentType", paths);
        Assert.DoesNotContain("Rules", paths);
    }

    [Fact]
    public void Set_failures_report_Rules()
    {
        var paths = PathsFor(Command(ContentMode.Dynamic, Fallback("https://a.io"), Fallback("https://b.io")));

        Assert.Contains("Rules", paths);
    }

    [Fact]
    public void Duplicate_orders_report_Rules()
    {
        var paths = PathsFor(Command(
            ContentMode.Dynamic,
            Conditional(1, "https://a.io"),
            Conditional(1, "https://b.io")));

        Assert.Contains("Rules", paths);
    }

    [Fact]
    public void Static_with_two_rules_reports_Mode()
    {
        var paths = PathsFor(Command(
            ContentMode.Static,
            Conditional(1, "https://a.io"),
            Fallback("https://b.io")));

        Assert.Contains("Mode", paths);
    }

    [Fact]
    public void Dangling_pointer_target_reports_Rules()
    {
        var rule = new DefaultPointerRuleValueObject { TargetOrder = 9 };
        var paths = PathsFor(Command(ContentMode.Dynamic, Conditional(1, "https://a.io"), rule));

        Assert.Contains("Rules", paths);
    }

    [Fact]
    public void Empty_rule_set_reports_Rules()
    {
        var paths = PathsFor(Command(ContentMode.Static));

        Assert.Contains("Rules", paths);
    }

    [Fact]
    public void Null_rules_content_and_style_report_fields_without_throwing()
    {
        Assert.Contains("Rules", PathsFor(Command(ContentMode.Static) with { Rules = null! }));
        Assert.Contains("Rules[0]", PathsFor(Command(ContentMode.Static, (CodeRuleValueObject)null!)));
        Assert.Contains("Rules[0].Content", PathsFor(Command(ContentMode.Static, new DefaultRuleValueObject { Content = null! })));
        Assert.Contains("Style", PathsFor(Command(ContentMode.Static, Fallback("https://a.io")) with { Style = null! }));
    }

    [Fact]
    public void Undefined_enums_are_rejected()
    {
        var command = Command((ContentMode)999, Fallback("https://a.io")) with
        {
            BarcodeFormat = (BarcodeFormat)999, ContentType = (CodeContentType)999,
        };
        var paths = PathsFor(command);
        Assert.Contains("Mode", paths);
        Assert.Contains("BarcodeFormat", paths);
        Assert.Contains("ContentType", paths);
    }

    [Fact]
    public void Update_uses_the_loaded_mode_and_preserves_wire_paths()
    {
        var request = new CodeUpdateCommand
        {
            Id = Guid.NewGuid(), UserId = Guid.NewGuid(), Name = "Name", ContentType = CodeContentType.Url,
            Rules = [Conditional(1, "https://a.io"), Fallback("https://b.io")], Style = StyleSpec.Default,
        };
        var validator = new CodeUpdateModelValidator();
        Assert.Contains(validator.Validate(new CodeUpdateModel(ContentMode.Static, request)).Errors, error => error.PropertyName == "Mode");
        Assert.True(validator.Validate(new CodeUpdateModel(ContentMode.Dynamic, request)).IsValid);
        request = request with { Rules = [Fallback("")], Name = "" };
        var paths = validator.Validate(new CodeUpdateModel(ContentMode.Static, request)).Errors.Select(error => error.PropertyName).ToArray();
        Assert.Contains("Name", paths);
        Assert.Contains("Rules[0].Content.Url", paths);
        Assert.DoesNotContain(paths, path => path.StartsWith("Command.", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(RuleConditionType.Device, "Ios", true)]
    [InlineData(RuleConditionType.Device, "999", false)]
    [InlineData(RuleConditionType.Country, "UZ", true)]
    [InlineData(RuleConditionType.Country, "ZZ", false)]
    [InlineData(RuleConditionType.Language, "en-US", true)]
    [InlineData(RuleConditionType.Language, " en ", false)]
    [InlineData(RuleConditionType.Language, "en-US\t", false)]
    [InlineData(RuleConditionType.Language, "en;q=0", false)]
    [InlineData(RuleConditionType.Language, "*", false)]
    [InlineData(RuleConditionType.TimeOfDay, "23:30-01:00", true)]
    [InlineData(RuleConditionType.TimeOfDay, "01:00-01:00", false)]
    [InlineData(RuleConditionType.TimeOfDay, "1 PM-3 PM", false)]
    [InlineData(RuleConditionType.TimeOfDay, "24:00-25:00", false)]
    public void Condition_values_follow_the_selected_signal(RuleConditionType condition, string value, bool valid)
    {
        var rule = Conditional(1, "https://a.io") with { Condition = condition, ConditionValue = value };
        Assert.Equal(valid, new ConditionalRuleValidator().Validate(rule).IsValid);
    }
}
