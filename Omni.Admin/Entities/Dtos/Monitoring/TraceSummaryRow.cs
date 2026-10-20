namespace Omni.Admin.Entities.Dtos.Monitoring;

/// <summary>One trace in the list (copy of the main app's PlatformAdmin TraceSummaryRow).</summary>
public record TraceSummaryRow(
    string TraceId,
    DateTimeOffset StartedAt,
    DateTimeOffset EndedAt,
    int Steps,
    int FailedSteps,
    int Errors,
    Guid? ClientId,
    Guid? ConversationId,
    string? Channel,
    string FirstStep,
    string? FailedStep,
    string? FailedDetail);