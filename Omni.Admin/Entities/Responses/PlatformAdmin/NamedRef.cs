namespace Omni.Admin.Entities.Responses.PlatformAdmin;

/// <summary>(Copy of the main app's Entities/Dtos/PlatformAdmin shape.) A related record shown by name on an admin page (client, service).</summary>
public record NamedRef(Guid Id, string Name);
