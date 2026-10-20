namespace Omni.Admin.Entities.Dtos.Content;

public record KnowledgeDocRow(Guid Id, Guid ClientId, string ClientName, string Title, string Category, string SourceType,
    string? SourceUrl, bool IsActive, int Chunks, int ContentLength, DateTimeOffset UpdatedAt);
