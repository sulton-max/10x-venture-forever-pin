namespace ForeverPin.Tests.E2E.Support;

/// <summary>Represents the wire shape of <c>CodeRuleValueObject</c>.</summary>
public sealed record RuleDtoModel
{
    public string Type { get; init; } = "";
    public int? Order { get; init; }
    public string? Condition { get; init; }
    public string? ConditionValue { get; init; }
    public int? TargetOrder { get; init; }
    public ContentDtoModel? Content { get; init; }
}
