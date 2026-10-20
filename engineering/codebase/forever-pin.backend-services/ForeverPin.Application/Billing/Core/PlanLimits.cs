using ForeverPin.Domain.Billing.Entities;
using ForeverPin.Domain.Billing.Enums;

namespace ForeverPin.Application.Billing.Core;

/// <summary>Contains what each plan holds: its dynamic codes and how far back its analytics reach.</summary>
/// <remarks>Static codes carry their content in the symbol and cost nothing to serve, so every plan holds unlimited ones.</remarks>
public static class PlanLimits
{
    /// <summary>Holds the sentinel for an unlimited cap on the wire.</summary>
    public const int Unlimited = -1;

    /// <summary>Holds the analytics window a paid plan reaches: a full year of days.</summary>
    public const int FullAnalyticsDays = 366;

    private static readonly IReadOnlyDictionary<Plan, int> MaxDynamicCodesByPlan = new Dictionary<Plan, int>
    {
        [Plan.Free] = 3,
        [Plan.Basic] = 25,
        [Plan.Pro] = 200,
        [Plan.Agency] = int.MaxValue,
    };

    private static readonly IReadOnlyDictionary<Plan, int> AnalyticsDaysByPlan = new Dictionary<Plan, int>
    {
        [Plan.Free] = 7,
        [Plan.Basic] = FullAnalyticsDays,
        [Plan.Pro] = FullAnalyticsDays,
        [Plan.Agency] = FullAnalyticsDays,
    };

    /// <summary>Gets the plan a subscription entitles today: a lapsed one entitles Free, and no row means Free.</summary>
    /// <remarks>A past-due subscription keeps its plan while Stripe retries the payment.</remarks>
    public static Plan EffectivePlan(SubscriptionEntity? subscription) => subscription?.Status switch
    {
        SubscriptionStatus.Active or SubscriptionStatus.Trialing or SubscriptionStatus.PastDue => subscription.Plan,
        _ => Plan.Free,
    };

    /// <summary>Gets the dynamic codes a plan may own; an unlimited plan returns <see cref="int.MaxValue"/>.</summary>
    public static int MaxDynamicCodes(Plan plan) =>
        MaxDynamicCodesByPlan.TryGetValue(plan, out var cap) ? cap : MaxDynamicCodesByPlan[Plan.Free];

    /// <summary>Gets the dynamic-code cap in API shape; an unlimited plan collapses to <see cref="Unlimited"/>.</summary>
    public static int MaxDynamicCodesForApi(Plan plan)
    {
        var cap = MaxDynamicCodes(plan);
        return cap == int.MaxValue ? Unlimited : cap;
    }

    /// <summary>Gets how many days back a plan's analytics reach.</summary>
    public static int AnalyticsDays(Plan plan) =>
        AnalyticsDaysByPlan.TryGetValue(plan, out var days) ? days : AnalyticsDaysByPlan[Plan.Free];
}
