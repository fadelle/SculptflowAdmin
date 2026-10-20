namespace Omni.Admin.Entities.Dtos.Monitoring;

/// <summary>One line of a message's timeline (copy of the main app's PlatformAdmin MessageTraceEntry). Kind: step,
/// message, status, billing, event, error or warning; Outcome: ok, skipped or failed; LogId opens a saved error.</summary>
public record MessageTraceEntry(
    DateTimeOffset At,
    string Kind,
    string Name,
    string Outcome,
    string Summary,
    double? DurationMs,
    Guid? ClientId,
    Guid? ConversationId,
    Guid? MessageId,
    string? N8nExecutionId,
    long? LogId,
    Dictionary<string, string?> Detail);