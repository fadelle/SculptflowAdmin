namespace Omni.Admin.Entities.Dtos.Monitoring;

/// <summary>(Copy of the main app's PlatformAdmin AiUsageRow.) One client's AI bot use: runs, model calls, tokens, the estimated
/// model cost (USD) and what it was charged for AI replies.</summary>
public record AiUsageRow(
    Guid ClientId,
    string ClientName,
    long Runs,
    long FailedRuns,
    long ModelCalls,
    long InputTokens,
    long OutputTokens,
    decimal EstimatedModelCostUsd,
    decimal AiReplyRevenue,
    string? RevenueCurrency);
