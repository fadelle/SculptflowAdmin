namespace Omni.Admin.Entities.Requests.Billing;

public record AdjustmentRequest(decimal Amount, string BalanceType, string Reason);
