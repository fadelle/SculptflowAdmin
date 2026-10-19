namespace Omni.Admin.Entities.Dtos.Monitoring;

/// <summary>One problem (fingerprint) on the Errors page (copy of the main app's PlatformAdmin AppLogGroupRow).
/// Occurrences counts each row's repeat count.</summary>
public record AppLogGroupRow(
    string Fingerprint,
    string Level,
    string Category,
    string? MessageTemplate,
    string LatestMessage,
    string? ExceptionType,
    long Occurrences,
    DateTimeOffset FirstSeenAt,
    DateTimeOffset LastSeenAt,
    long LatestId,
    Guid? LatestClinicId);