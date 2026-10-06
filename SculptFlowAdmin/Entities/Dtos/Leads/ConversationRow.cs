namespace SculptFlowAdmin.Entities.Dtos.Leads;

public record ConversationRow(Guid Id, Guid ClinicId, string ClinicName, Guid LeadId, string LeadName, string Channel,
    string Status, string Mode, DateTimeOffset? LastMessageAt, string? LastMessageDirection, int MessageCount, DateTimeOffset CreatedAt);
