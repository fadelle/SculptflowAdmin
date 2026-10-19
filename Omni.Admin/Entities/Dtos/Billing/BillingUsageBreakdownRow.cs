namespace Omni.Admin.Entities.Dtos.Billing;

public record BillingUsageBreakdownRow(string EventType, string Label, int Count, decimal Quantity, decimal Amount);
