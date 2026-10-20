namespace Omni.Admin.Entities.Dtos.Overview;

public record ProblemRow(string Kind, Guid ClientId, string ClientName, string Title, string? Detail, DateTimeOffset? At, string Link);
