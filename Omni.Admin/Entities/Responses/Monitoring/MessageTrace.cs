using Omni.Admin.Entities.Dtos.Monitoring;

namespace Omni.Admin.Entities.Responses.Monitoring;

/// <summary>GET /api/platform-admin/message-traces/{traceId} and by-message/{id} (copy of the main app's PlatformAdmin
/// MessageTrace): one message's whole story, oldest first.</summary>
public record MessageTrace(
    string TraceId,
    DateTimeOffset? StartedAt,
    DateTimeOffset? EndedAt,
    Guid? ClinicId,
    List<Guid> ConversationIds,
    List<Guid> MessageIds,
    bool HasFailure,
    List<MessageTraceEntry> Entries);