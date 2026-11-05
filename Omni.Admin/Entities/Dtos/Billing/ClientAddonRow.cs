namespace Omni.Admin.Entities.Dtos.Billing;

/// <summary>(Copy of the main app's Entities/Dtos/Billing shape.) One add-on a client has: what it raises, how many units
/// and what they cost per period.</summary>
/// <param name="AutoRenew">As chosen for this add-on: true, false, or null = follow the main app's Billing:AddonsRenewByDefault.</param>
/// <param name="Renews">Whether it renews at the next renewal (the choice, else the default). One that doesn't ends then.</param>
public record ClientAddonRow(string Code, string Name, string EntitlementKey, int Quantity, decimal UnitPrice, string Currency, decimal PeriodPrice,
    bool? AutoRenew = null, bool Renews = false);
