namespace Omni.Admin.Entities.Requests.Billing;

/// <summary>(Copy of the main app's shape.) AllowAnotherTrial: start the trial plan for a client that already had one
/// (QA t2-billing-21: one trial per client otherwise).</summary>
public record StartSubscriptionApiRequest(string PlanCode, bool ChargeFirstPeriod = true, string? Reason = null, bool AllowAnotherTrial = false);
