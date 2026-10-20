namespace Omni.Admin.Entities.Dtos.Admins;

/// <summary>The outcome of a portal sign-in: the admin, or the message for the sign-in form (the portal's own shape).</summary>
public record AdminSignInResult(AdminIdentity? Admin, string? Error);
