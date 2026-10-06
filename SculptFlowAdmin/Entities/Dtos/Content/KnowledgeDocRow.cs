namespace SculptFlowAdmin.Entities.Dtos.Content;

public record KnowledgeDocRow(Guid Id, Guid ClinicId, string ClinicName, string Title, string Category, string SourceType,
    string? SourceUrl, bool IsActive, int Chunks, int ContentLength, DateTimeOffset UpdatedAt);
