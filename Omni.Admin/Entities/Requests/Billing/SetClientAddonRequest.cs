namespace Omni.Admin.Entities.Requests.Billing;

/// <summary>(Copy of the main app's shape.) PUT billing/clients/{id}/addons/{code}: the client's new number of units
/// (0 removes the add-on). Added units are charged from the wallet now (Idempotency-Key).</summary>
public record SetClientAddonRequest(int Quantity);
