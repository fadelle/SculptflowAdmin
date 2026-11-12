using Omni.Admin.Entities.Dtos.Monitoring;

namespace Omni.Admin.Entities.Responses.Monitoring;

/// <summary>GET /api/platform-admin/message-traces/{traceId} and by-message/{id} (copy of the main app's PlatformAdmin
/// MessageTrace): one message's whole story, oldest first.</summary>
public record MessageTrace(
    string TraceId,
    DateTimeOffset? StartedAt,
    DateTimeOffset? EndedAt,
    Guid? ClientId,
    List<Guid> ConversationIds,
    List<Guid> MessageIds,
    bool HasFailure,
    List<MessageTraceEntry> Entries,
    // What the trace came to and why (main app TraceResultRule; added 2026-11-07, QA t6-admin-28). "other" from an older main app.
    string Outcome = "other",
    string? OutcomeReason = null);