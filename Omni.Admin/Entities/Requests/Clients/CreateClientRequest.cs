namespace Omni.Admin.Entities.Requests.Clients;

/// <summary>(Copy of the main app's Entities/Requests/PlatformAdmin shape.) Opens a client for a sale (POST clients): its name,
/// business type and dashboard language, an optional plan (null = the sign-up plan), and the owner who gets an emailed invite.</summary>
public record CreateClientRequest(
    string? Name,
    string? BusinessTypeCode,
    string? OwnerEmail,
    string? OwnerName,
    string? PlanCode,
    string? Language
);
