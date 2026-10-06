namespace SculptFlowAdmin.Entities.Dtos.Billing;

public record AdminUsageBreakdownRow(string EventType, string Channel, string ProviderBilling, int Count, decimal Quantity, decimal Amount,
    decimal ProviderCost, decimal Margin);
