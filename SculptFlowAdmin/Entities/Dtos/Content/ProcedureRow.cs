namespace SculptFlowAdmin.Entities.Dtos.Content;

public record ProcedureRow(Guid Id, Guid ClinicId, string ClinicName, string Name, string? Code, int? ConsultationDuration,
    bool IsActive, int Leads, DateTimeOffset UpdatedAt);
