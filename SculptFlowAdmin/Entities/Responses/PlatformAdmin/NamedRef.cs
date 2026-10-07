namespace SculptFlowAdmin.Entities.Responses.PlatformAdmin;

/// <summary>(Copy of the main app's Entities/Dtos/PlatformAdmin shape.) A related record shown by name on an admin page (clinic, procedure).</summary>
public record NamedRef(Guid Id, string Name);
