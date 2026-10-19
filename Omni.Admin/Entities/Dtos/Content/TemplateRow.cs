namespace Omni.Admin.Entities.Dtos.Content;

public record TemplateRow(Guid Id, Guid ClinicId, string ClinicName, string Name, string Category, string Language, string Status,
    string? QualityRating, string? RejectionReason, string Body, DateTimeOffset UpdatedAt);
