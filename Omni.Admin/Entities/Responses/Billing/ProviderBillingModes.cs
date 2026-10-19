using Omni.Admin.Entities.Dtos.Billing;

namespace Omni.Admin.Entities.Responses.Billing;

public record ProviderBillingModes(IReadOnlyList<ProviderBillingMode> Modes, IReadOnlyList<ProviderBillingDefault> Defaults);
