namespace Omni.Admin.Entities.Dtos.Billing;

public record ProviderDirectUsageRow(string EventType, string Label, string PaidBy, int Count, decimal Quantity);
