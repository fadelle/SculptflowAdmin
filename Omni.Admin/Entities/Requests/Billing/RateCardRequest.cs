namespace Omni.Admin.Entities.Requests.Billing;

public record RateCardRequest(string Code, string Name, string? Description, Guid? ClientId, bool IsDefault = false);
