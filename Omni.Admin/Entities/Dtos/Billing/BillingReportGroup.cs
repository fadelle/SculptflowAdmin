namespace Omni.Admin.Entities.Dtos.Billing;

/// <summary>One line of the revenue report after grouping (by client, channel, event or who pays the provider).</summary>
public record BillingReportGroup(string Key, string Label, Guid? ClientId, int Count, decimal Quantity, decimal Revenue,
    decimal ProviderCost, decimal Margin);
