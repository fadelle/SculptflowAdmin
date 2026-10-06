namespace SculptFlowAdmin.Entities.Requests.Billing;

public record StartSubscriptionApiRequest(string PlanCode, bool ChargeFirstPeriod = true, string? Reason = null);
