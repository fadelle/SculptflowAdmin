namespace Omni.Admin.Entities.Dtos.Billing;

public record ClientChannelBillingRow(string Channel, string ChannelLabel, string? DisplayName, string PaidBy, bool ChargedByPlatform);
