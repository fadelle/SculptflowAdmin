namespace SculptFlowAdmin.Entities.Dtos.Billing;

public record ProviderDirectUsageRow(string EventType, string Label, string PaidBy, int Count, decimal Quantity);
