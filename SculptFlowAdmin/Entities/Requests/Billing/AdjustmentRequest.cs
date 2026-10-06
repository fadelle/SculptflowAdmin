namespace SculptFlowAdmin.Entities.Requests.Billing;

public record AdjustmentRequest(decimal Amount, string BalanceType, string Reason);
