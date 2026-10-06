using SculptFlowAdmin.Entities.Dtos.Billing;

namespace SculptFlowAdmin.Entities.Responses.Billing;

public record BillingReport(DateTimeOffset From, DateTimeOffset To, string Currency, IReadOnlyList<BillingReportRow> Usage,
    decimal UsageRevenue, decimal ProviderCostPaidBySculptFlow, decimal ProviderCostPaidExternally, decimal Margin,
    decimal SubscriptionRevenue, decimal TopUps);
