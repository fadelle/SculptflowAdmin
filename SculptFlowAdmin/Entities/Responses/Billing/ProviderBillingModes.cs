using SculptFlowAdmin.Entities.Dtos.Billing;

namespace SculptFlowAdmin.Entities.Responses.Billing;

public record ProviderBillingModes(IReadOnlyList<ProviderBillingMode> Modes, IReadOnlyList<ProviderBillingDefault> Defaults);
