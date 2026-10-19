namespace Omni.Admin.Entities.Dtos.Overview;

public record ProblemRow(string Kind, Guid ClinicId, string ClinicName, string Title, string? Detail, DateTimeOffset? At, string Link);
