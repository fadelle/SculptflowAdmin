namespace Omni.Admin.Entities.Dtos.Content;

public record TemplateRow(Guid Id, Guid ClientId, string ClientName, string Name, string Category, string Language, string Status,
    string? QualityRating, string? RejectionReason, string Body, DateTimeOffset UpdatedAt);
