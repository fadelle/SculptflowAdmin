namespace SculptFlowAdmin.Entities.Responses.PlatformAdmin;

/// <summary>(Copy of the main app's Entities/Dtos/PlatformAdmin shape.) What a platform-admin write touched, so the portal can file its audit entry under the right clinic.</summary>
public record PlatformAdminChange(Guid? ClinicId);
