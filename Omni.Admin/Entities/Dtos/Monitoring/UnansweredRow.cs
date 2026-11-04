namespace Omni.Admin.Entities.Dtos.Monitoring;

/// <summary>(Copy of the main app's PlatformAdmin UnansweredRow.) A conversation whose last message is the customer's and got no
/// answer, with what that message's trace came to.</summary>
public record UnansweredRow(
    Guid ConversationId,
    Guid ClientId,
    string ClientName,
    string? ContactName,
    string Channel,
    string Mode,
    DateTimeOffset LastCustomerMessageAt,
    Guid? MessageId,
    string? MessageType,
    string? TraceId,
    string? Outcome,
    string? OutcomeReason);
