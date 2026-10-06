namespace SculptFlowAdmin.Entities.Requests.Billing;

public record AddRateRequest(
    string EventType,
    string? CountryCode,
    string? Operator,
    string? Provider,
    string? Unit,
    decimal ProviderCost,
    decimal ClientRate,
    DateTimeOffset? EffectiveFrom = null,
    DateTimeOffset? EffectiveTo = null,
    string? Notes = null,
    string? ProviderBilling = null);
