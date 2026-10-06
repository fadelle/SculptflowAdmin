using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;

namespace SculptFlowAdmin.Common.Statics;

/// <summary>Who is acting, read from the admin cookie.</summary>
public static class AdminClaims
{
    public static Guid? Id(ClaimsPrincipal user) =>
        Guid.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    public static string Email(ClaimsPrincipal user) => user.FindFirstValue(ClaimTypes.Email) ?? "unknown";
    public static string Name(ClaimsPrincipal user) => user.FindFirstValue(ClaimTypes.Name) ?? Email(user);
}
