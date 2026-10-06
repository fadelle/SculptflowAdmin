namespace SculptFlowAdmin.Entities.Dtos.Billing;

/// <summary>One connected channel account with who pays the provider and whether SculptFlow charges its usage.</summary>
public record ChannelAccountBilling(
    Guid ChannelIntegrationId, string Channel, string? Provider, string Status, string? DisplayName,
    string ProviderBilling, string ProviderBillingLabel, bool OmniUsageBilling, string Source,
    string DefaultProviderBilling, string? OverrideProviderBilling, bool? OverrideOmniUsageBilling, string? OverrideAppliesToProvider,
    string? OverrideReason, string? OverrideUpdatedBy, DateTimeOffset? OverrideUpdatedAt);
