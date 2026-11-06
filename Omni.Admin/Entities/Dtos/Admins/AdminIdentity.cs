namespace Omni.Admin.Entities.Dtos.Admins;

/// <summary>Who an admin is: the answer of the main app's POST /api/platform-admin/auth/login and GET .../auth/admins/{id}
/// (main app: <c>Entities/Dtos/PlatformAdmin/AdminIdentity</c>). The portal puts these in its own cookie. SessionStamp changes when
/// the admin's password is set, which signs their open sessions out (QA t6-admin-13).</summary>
public record AdminIdentity(string Id, string? FullName, string? Email, string? SessionStamp = null);
