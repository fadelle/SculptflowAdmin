namespace Omni.Admin.Entities.Dtos.Billing;

public record BillingReportRow(Guid ClientId, string ClientName, string Channel, string EventType, string ProviderBilling, int Count,
    decimal Quantity, decimal Revenue, decimal ProviderCost, decimal Margin);
