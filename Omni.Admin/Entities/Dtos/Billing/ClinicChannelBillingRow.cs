namespace Omni.Admin.Entities.Dtos.Billing;

public record ClinicChannelBillingRow(string Channel, string ChannelLabel, string? DisplayName, string PaidBy, bool ChargedByPlatform);
