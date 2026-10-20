namespace Omni.Admin.Entities.Dtos.Billing;

public record BillingAccountRow(Guid ClientId, string ClientName, string? PlanCode, string? SubscriptionStatus, DateTimeOffset? CurrentPeriodEnd,
    decimal WalletBalance, decimal IncludedCreditBalance, decimal ReservedAmount, string Currency);
