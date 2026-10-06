namespace SculptFlowAdmin.Entities.Dtos.Leads;

public record AppointmentRow(Guid Id, Guid ClinicId, string ClinicName, string ClinicTimezone, Guid LeadId, string LeadName,
    string? ProcedureName, string Status, DateTimeOffset ScheduledStart, DateTimeOffset? ScheduledEnd, string? Notes,
    DateTimeOffset CreatedAt);
