using ForeverPin.Application.Billing.Core;
using ForeverPin.Domain.Billing.Entities;
using ForeverPin.Domain.Billing.Enums;

namespace ForeverPin.Tests.Unit;

/// <summary>Unit tests for what each plan holds and which plan a subscription entitles.</summary>
public class PlanLimitsTests
{
    [Theory]
    [InlineData(Plan.Free, 3)]
    [InlineData(Plan.Basic, 25)]
    [InlineData(Plan.Pro, 200)]
    public void MaxDynamicCodes_ShouldReturnTheCap_ForBoundedPlans(Plan plan, int expected) =>
        Assert.Equal(expected, PlanLimits.MaxDynamicCodes(plan));

    [Fact]
    public void MaxDynamicCodes_ShouldBeUnlimitedInternally_ForAgency() =>
        Assert.Equal(int.MaxValue, PlanLimits.MaxDynamicCodes(Plan.Agency));

    [Fact]
    public void MaxDynamicCodesForApi_ShouldCollapseToMinusOne_ForAgency() =>
        Assert.Equal(-1, PlanLimits.MaxDynamicCodesForApi(Plan.Agency));

    [Theory]
    [InlineData(Plan.Free, 7)]
    [InlineData(Plan.Basic, 366)]
    public void AnalyticsDays_ShouldReturnTheReach_ForEachPlan(Plan plan, int expected) =>
        Assert.Equal(expected, PlanLimits.AnalyticsDays(plan));

    [Theory]
    [InlineData(SubscriptionStatus.Active, Plan.Basic)]
    [InlineData(SubscriptionStatus.Trialing, Plan.Basic)]
    [InlineData(SubscriptionStatus.PastDue, Plan.Basic)]
    [InlineData(SubscriptionStatus.Canceled, Plan.Free)]
    [InlineData(SubscriptionStatus.Unpaid, Plan.Free)]
    [InlineData(SubscriptionStatus.Incomplete, Plan.Free)]
    public void EffectivePlan_ShouldFollowTheSubscriptionStatus(SubscriptionStatus status, Plan expected) =>
        Assert.Equal(expected, PlanLimits.EffectivePlan(new SubscriptionEntity
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Plan = Plan.Basic,
            Status = status,
            StripeCustomerId = "cus",
            StripeSubscriptionId = "sub",
        }));

    [Fact]
    public void EffectivePlan_ShouldBeFree_WithoutASubscription() =>
        Assert.Equal(Plan.Free, PlanLimits.EffectivePlan(null));
}
