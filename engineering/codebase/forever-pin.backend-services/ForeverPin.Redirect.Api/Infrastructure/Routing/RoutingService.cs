using System.Globalization;
using ForeverPin.Domain.Codes.Content;
using ForeverPin.Domain.Codes.Content.Url.Models;
using ForeverPin.Domain.Codes.Content.MobileApp.Models;
using ForeverPin.Domain.Codes.Content.Phone.Models;
using ForeverPin.Domain.Codes.Content.Sms.Models;
using ForeverPin.Domain.Codes.Content.Email.Models;
using ForeverPin.Domain.Codes.Content.Geo.Models;
using ForeverPin.Domain.Codes.Rules.Mappers;
using WoW.Two.Sdk.Backend.Beta.Foundation.Results;
using ForeverPin.Domain.Codes.Core.Entities;
using ForeverPin.Domain.Codes.Core.Enums;
using ForeverPin.Domain.Codes.Rules.Models;
using ForeverPin.Redirect.Api.Application.Routing.Models;
using ForeverPin.Redirect.Api.Application.Routing.Services;

namespace ForeverPin.Redirect.Api.Infrastructure.Routing;

/// <summary>Provides rule evaluation for a scan.</summary>
/// <remarks>Resolve the scan context (device, geo, language) before evaluating.</remarks>
public sealed class RoutingService(CodePayloadMapper payloads) : IRoutingService
{
    /// <inheritdoc />
    public RoutingResult Evaluate(CodeEntity code, ScanContextModel context)
    {
        if (!code.IsActive)
            return new RoutingResult.NotFound(RoutingMiss.Paused);

        var conditional = code.Rules.OfType<ConditionalRuleValueObject>().OrderBy(rule => rule.Order);
        foreach (var rule in conditional)
        {
            if (Matches(rule, context))
                return Resolve(rule.Content, rule.Order);
        }

        // No conditional rule matched — the catch-all serves the scan, or the code deliberately does not resolve.
        return code.Rules.FirstOrDefault(rule => rule is DefaultRuleValueObject or DefaultPointerRuleValueObject) switch
        {
            DefaultRuleValueObject fallback => Resolve(fallback.Content, null),
            DefaultPointerRuleValueObject pointer => ResolvePointer(code, pointer),
            _ => new RoutingResult.NotFound(),
        };
    }

    // The pointer nominates an existing conditional rule rather than repeating its content.
    private RoutingResult ResolvePointer(CodeEntity code, DefaultPointerRuleValueObject pointer)
    {
        var target = code.Rules
            .OfType<ConditionalRuleValueObject>()
            .FirstOrDefault(rule => rule.Order == pointer.TargetOrder);

        return target is null
            ? new RoutingResult.NotFound()
            : Resolve(target.Content, target.Order);
    }

    private RoutingResult Resolve(CodeContentValueObject content, int? matchedRuleOrder)
    {
        if (content is not (UrlContentValueObject or MobileAppLinkContentValueObject or PhoneContentValueObject
            or SmsContentValueObject or EmailContentValueObject or GeoContentValueObject))
            return new RoutingResult.NotFound(RoutingMiss.Undeliverable);

        var payload = payloads.Map(content);
        if (payload is not Result<string>.Success success || string.IsNullOrWhiteSpace(success.Value))
            return new RoutingResult.NotFound();

        // Existing URI payloads retain their schemes; document/text delivery has no redirect representation.
        if (!Uri.TryCreate(success.Value, UriKind.Absolute, out var destination)
            || destination.Scheme is not ("http" or "https" or "tel" or "mailto" or "smsto" or "geo"))
            return new RoutingResult.NotFound(RoutingMiss.Undeliverable);

        return new RoutingResult.Redirect(success.Value, matchedRuleOrder);
    }

    private static bool Matches(ConditionalRuleValueObject rule, ScanContextModel ctx) => rule.Condition switch
    {
        RuleConditionType.Device => string.Equals(
            rule.ConditionValue,
            ctx.Device.ToString(),
            StringComparison.OrdinalIgnoreCase),
        RuleConditionType.Country => ctx.CountryCode is not null
            && string.Equals(rule.ConditionValue, ctx.CountryCode, StringComparison.OrdinalIgnoreCase),
        RuleConditionType.Language => MatchesLanguage(rule.ConditionValue, ctx),
        RuleConditionType.TimeOfDay => MatchesTimeWindow(rule.ConditionValue, ctx.NowUtc),
        _ => false,
    };

    private static bool MatchesLanguage(string language, ScanContextModel context)
    {
        if (context.LanguagePreferences.Count == 0)
            return context.Language is not null && string.Equals(language, context.Language, StringComparison.OrdinalIgnoreCase);

        foreach (var preference in context.LanguagePreferences.Where(item => item.Quality > 0 && item.Range != "*"))
        {
            if (!Includes(language, preference.Range) && !Includes(preference.Range, language)) continue;
            var tag = language.Length >= preference.Range.Length ? language : preference.Range;
            var effective = context.LanguagePreferences
                .Where(item => item.Range == "*" || Includes(item.Range, tag))
                .OrderByDescending(item => item.Range == "*" ? 0 : item.Range.Length)
                .First();
            if (effective.Quality > 0) return true;
        }
        return false;
    }

    private static bool Includes(string range, string tag) => string.Equals(range, tag, StringComparison.OrdinalIgnoreCase)
        || tag.StartsWith(range + "-", StringComparison.OrdinalIgnoreCase);

    /// <summary>Matches a daily UTC <c>HH:mm-HH:mm</c> window, wrapping past midnight.</summary>
    private static bool MatchesTimeWindow(string? window, DateTimeOffset now)
    {
        if (string.IsNullOrWhiteSpace(window))
            return false;

        var parts = window.Split('-', 2);
        if (parts.Length != 2
            || !TimeOnly.TryParseExact(parts[0], "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var start)
            || !TimeOnly.TryParseExact(parts[1], "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var end))
            return false;

        var t = TimeOnly.FromDateTime(now.UtcDateTime);
        return start <= end
            ? t >= start && t < end
            : t >= start || t < end;
    }
}
