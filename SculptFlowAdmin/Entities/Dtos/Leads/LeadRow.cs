namespace SculptFlowAdmin.Entities.Dtos.Leads;

public record LeadRow(Guid Id, Guid ClinicId, string ClinicName, string Name, string? Phone, string? Email, string? Source,
    string Status, string QualificationStatus, string? ProcedureName, bool MarketingOptIn, DateTimeOffset? LastContactAt,
    DateTimeOffset CreatedAt);
