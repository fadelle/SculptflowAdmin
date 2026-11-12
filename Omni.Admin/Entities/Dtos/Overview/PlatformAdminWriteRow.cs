namespace Omni.Admin.Entities.Dtos.Overview;

/// <summary>(Copy of the main app's PlatformAdminWrite.) One write to the main app's platform-admin API as the main app recorded
/// it (QA t6-admin-23): the actor the X-Admin-Actor header named, the caller's address, the request and its status.</summary>
public record PlatformAdminWriteRow(long Id, DateTimeOffset OccurredAt, string Actor, string? IpAddress, string Method, string Path,
    int StatusCode, Guid? ClientId);
