using Microsoft.EntityFrameworkCore;
using ForeverPin.Domain.Codes.Core.Entities;
using ForeverPin.Persistence.Configurations;
using WoW.Two.Sdk.Beta.Be.Data.EntityFrameworkCore.Json;
using ForeverPin.Domain.Codes.Serialization.Factories;
using System.Text.Json;
using ForeverPin.Domain.Codes.Content.Url.Models;
using ForeverPin.Domain.Codes.Content.Wifi.Models;
using ForeverPin.Common.Domain.Codes.Content.Wifi.Enums;
using ForeverPin.Domain.Codes.Core.Enums;
using ForeverPin.Domain.Codes.Rules.Mappers;
using ForeverPin.Domain.Codes.Rules.Models;

namespace ForeverPin.Tests.Unit;

/// <summary>Proves the wire and jsonb contract — outer union and nested content.</summary>
/// <remarks><see cref="CodeContentJsonTests"/> covers the inner union alone; this covers both bound together.</remarks>
public sealed class CodeRuleJsonTests
{
    private static ConditionalRuleValueObject Conditional() => new()
    {
        Order = 2,
        Condition = RuleConditionType.Device,
        ConditionValue = "Ios",
        Content = new UrlContentValueObject { Url = "https://ios.example.com" },
    };

    public static TheoryData<CodeRuleValueObject, string> Cases() => new()
    {
        { Conditional(), "conditional" },
        {
            new DefaultRuleValueObject { Content = new UrlContentValueObject { Url = "https://fallback.example.com" } },
            "default"
        },
        { new DefaultPointerRuleValueObject { TargetOrder = 2 }, "defaultPointer" },
    };

    [Theory]
    [MemberData(nameof(Cases))]
    public void Serializes_with_the_camelCase_type_discriminator(CodeRuleValueObject rule, string discriminator)
    {
        var json = Serialize([rule]);

        using var document = JsonDocument.Parse(json);
        Assert.Equal(discriminator, document.RootElement[0].GetProperty("type").GetString());
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public void Round_trips_each_role_back_to_its_concrete_type(CodeRuleValueObject rule, string discriminator)
    {
        _ = discriminator;

        var restored = Deserialize(Serialize([rule]));

        Assert.Equal(rule, Assert.Single(restored));
        Assert.IsType(rule.GetType(), restored[0]);
    }

    [Fact]
    public void Round_trips_a_whole_rule_set_in_order()
    {
        List<CodeRuleValueObject> rules =
        [
            Conditional(),
            new ConditionalRuleValueObject
            {
                Order = 1,
                Condition = RuleConditionType.Country,
                ConditionValue = "UZ",
                Content = new WifiContentValueObject
                {
                    Ssid = "Cafe",
                    Password = "pw",
                    Encryption = WifiEncryption.Wpa
                },
            },
            new DefaultPointerRuleValueObject { TargetOrder = 1 },
        ];

        var restored = Deserialize(Serialize(rules));

        Assert.Equal(rules, restored);
    }

    [Fact]
    public void Nested_content_keeps_its_own_discriminator()
    {
        var rule = new DefaultRuleValueObject { Content = new WifiContentValueObject
        {
            Ssid = "Cafe",
            Encryption = WifiEncryption.Wpa
        } };

        var json = Serialize([rule]);

        using var document = JsonDocument.Parse(json);
        var element = document.RootElement[0];
        Assert.Equal("default", element.GetProperty("type").GetString());
        Assert.Equal("wifi", element.GetProperty("content").GetProperty("type").GetString());
    }

    [Fact]
    public void Deserialize_null_or_blank_returns_an_empty_list()
    {
        var model = new ModelBuilder();
        new CodeEntityConfiguration().Configure(model.Entity<CodeEntity>());
        var converter = model.Model.FindEntityType(typeof(CodeEntity))!.FindProperty(nameof(CodeEntity.Rules))!.GetValueConverter()!;
        Assert.Empty((List<CodeRuleValueObject>)converter.ConvertFromProvider("null")!);
        Assert.Empty((List<CodeRuleValueObject>)converter.ConvertFromProvider("  ")!);
    }
    private static string Serialize(List<CodeRuleValueObject> value) => JsonSerializer.Serialize(value, CodeJsonOptionsFactory.CreateRules());
    private static List<CodeRuleValueObject> Deserialize(string json) => JsonSerializer.Deserialize<List<CodeRuleValueObject>>(json, CodeJsonOptionsFactory.CreateRules()) ?? [];

    [Fact]
    public void Ef_uses_the_sdk_comparer_with_independent_nested_snapshots()
    {
        var model = new ModelBuilder();
        new CodeEntityConfiguration().Configure(model.Entity<CodeEntity>());
        var comparer = Assert.IsType<JsonValueComparer<List<CodeRuleValueObject>>>(model.Model.FindEntityType(typeof(CodeEntity))!
            .FindProperty(nameof(CodeEntity.Rules))!.GetValueComparer());
        var rules = new List<CodeRuleValueObject> { Conditional() };
        var snapshot = comparer.Snapshot(rules);
        Assert.NotSame(rules, snapshot);
        Assert.NotSame(rules[0], snapshot[0]);
        Assert.NotSame(((ConditionalRuleValueObject)rules[0]).Content, ((ConditionalRuleValueObject)snapshot[0]).Content);
        Assert.True(comparer.Equals(rules, snapshot));
        rules[0] = Conditional() with { Content = new UrlContentValueObject { Url = "https://changed.test" } };
        Assert.False(comparer.Equals(rules, snapshot));
    }
}
