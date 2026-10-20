namespace Omni.Admin.Entities.Dtos.Monitoring;

/// <summary>One saved warning/error in a list (copy of the main app's PlatformAdmin AppLogRow).</summary>
public record AppLogRow(
    long Id,
    DateTimeOffset OccurredAt,
    string Level,
    string Category,
    string Message,
    string? ExceptionType,
    string Fingerprint,
    int RepeatCount,
    string? TraceId,
    Guid? ClientId,
    Guid? ConversationId,
    Guid? MessageId,
    string? Method,
    string? Route);