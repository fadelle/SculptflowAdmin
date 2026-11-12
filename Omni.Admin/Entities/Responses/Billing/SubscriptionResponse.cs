namespace Omni.Admin.Entities.Responses.Billing;

public record SubscriptionResponse(Guid Id, Guid ClientId, string PlanCode, string PlanName, string Status,
    DateTimeOffset CurrentPeriodStart, DateTimeOffset CurrentPeriodEnd, bool CancelAtPeriodEnd, DateTimeOffset? PastDueSince,
    DateTimeOffset? EndedAt)
{
    /// <summary>After a plan change: the limits the client now uses more of than the plan allows (QA t2-billing-11).</summary>
    public IReadOnlyList<OverLimitRow>? OverLimits { get; init; }
}

/// <summary>(Copy of the main app's PlanQuotaRow.) One limit: in use and the plan's limit (null = unlimited).</summary>
public record OverLimitRow(string Key, int Used, int? Limit);
