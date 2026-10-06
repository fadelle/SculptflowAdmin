namespace SculptFlowAdmin.Entities.Dtos.Billing;

public record BillingReportRow(Guid ClinicId, string ClinicName, string Channel, string EventType, string ProviderBilling, int Count,
    decimal Quantity, decimal Revenue, decimal ProviderCost, decimal Margin);
