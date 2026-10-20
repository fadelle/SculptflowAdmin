namespace Omni.Admin.Entities.Responses.Monitoring;

/// <summary>GET /api/platform-admin/logs/{id} (copy of the main app's PlatformAdmin AppLogDetail): one row in full,
/// secrets already masked by the main app.</summary>
public record AppLogDetail(
    long Id,
    DateTimeOffset OccurredAt,
    string Level,
    string Category,
    string Message,
    string? MessageTemplate,
    string? ExceptionType,
    string? ExceptionMessage,
    string? StackTrace,
    string Fingerprint,
    int RepeatCount,
    string? TraceId,
    Guid? ClientId,
    Guid? ConversationId,
    Guid? MessageId,
    string? Method,
    string? Route,
    string? Instance,
    Dictionary<string, string?> Properties);