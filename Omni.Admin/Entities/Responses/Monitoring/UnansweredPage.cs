using Omni.Admin.Entities.Dtos.Monitoring;

namespace Omni.Admin.Entities.Responses.Monitoring;

/// <summary>(Copy of the main app's PlatformAdmin UnansweredPage.) Conversations left without an answer, oldest wait first.</summary>
public record UnansweredPage(DateTimeOffset From, DateTimeOffset Before, int MinutesWithoutAnswer, int Page, int PageSize, int Total,
    IReadOnlyList<UnansweredRow> Items);
