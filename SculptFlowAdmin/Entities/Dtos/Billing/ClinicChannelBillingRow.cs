namespace SculptFlowAdmin.Entities.Dtos.Billing;

public record ClinicChannelBillingRow(string Channel, string ChannelLabel, string? DisplayName, string PaidBy, bool ChargedBySculptFlow);
