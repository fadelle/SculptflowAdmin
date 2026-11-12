namespace Omni.Admin.Entities.Dtos.Content;

/// <summary>(Copy of the main app's Entities/Dtos/Knowledge KnowledgeReindexProgress, QA t4-ai-15.) How far a client's
/// Knowledge Base re-index is; kept in the main app's memory since it started.</summary>
public sealed record KnowledgeReindexProgress(int Total, int Done, int Failed, DateTimeOffset StartedAt, DateTimeOffset? FinishedAt, bool Running);
