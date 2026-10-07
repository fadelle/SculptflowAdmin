namespace SculptFlowAdmin.Entities.Responses.PlatformAdmin;

/// <summary>(Copy of the main app's Entities/Dtos/PlatformAdmin shape.) One knowledge document with its content.</summary>
public record KnowledgeDocDetail(Guid Id, Guid ClinicId, NamedRef? Clinic, string Title, string Category, string Content,
    string SourceType, string? OriginalFileName, string? MimeType, long? FileSizeBytes, string? SourceUrl, bool IsActive,
    DateTimeOffset UpdatedAt);
