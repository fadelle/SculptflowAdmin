using System.Security.Claims;

namespace Omni.Admin.Common.Statics;

/// <summary>Who is acting, read from the admin cookie. The id is the main app's identity.users id (text).</summary>
public static class AdminClaims
{
    public static string? Id(ClaimsPrincipal user) =>
        user.FindFirstValue(ClaimTypes.NameIdentifier) is { Length: > 0 } id ? id : null;

    public static string Email(ClaimsPrincipal user) => user.FindFirstValue(ClaimTypes.Email) is { Length: > 0 } e ? e : "unknown";
    public static string Name(ClaimsPrincipal user) => user.FindFirstValue(ClaimTypes.Name) is { Length: > 0 } n ? n : Email(user);
}
