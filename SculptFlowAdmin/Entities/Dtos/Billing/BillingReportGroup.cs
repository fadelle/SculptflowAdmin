namespace SculptFlowAdmin.Entities.Dtos.Billing;

/// <summary>One line of the revenue report after grouping (by clinic, channel, event or who pays the provider).</summary>
public record BillingReportGroup(string Key, string Label, Guid? ClinicId, int Count, decimal Quantity, decimal Revenue,
    decimal ProviderCost, decimal Margin);
