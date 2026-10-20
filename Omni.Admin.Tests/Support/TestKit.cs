using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Omni.Admin.Business.Contracts.HttpClients.MainApp;
using Omni.Admin.Business.Services.Auth;
using Omni.Admin.Common.Configs;
using Omni.Admin.Entities.Dtos.Admins;

namespace Omni.Admin.Tests.Support;

/// <summary>Shared builders for the portal's tests.</summary>
public static class TestKit
{
    public const string BaseUrl = "https://main.test";
    public const string ApiKey = "test-platform-admin-key";

    /// <summary>The main app's key filter answer (its text names the header).</summary>
    public const string KeyRejectedBody = """{"error":"Missing or invalid X-Platform-Admin-Key header."}""";

    /// <summary>The main app's answer for every refused admin sign-in.</summary>
    public const string SignInRefusedBody = """{"error":"Wrong email or password, or the account is locked for a few minutes."}""";

    public static readonly AdminIdentity Ada = new("ad000000-0000-4000-8000-000000000001", "Ada Admin", "ada@omni.test");

    public static IOptions<MainAppApiOptions> Options(string? key = ApiKey) =>
        Microsoft.Extensions.Options.Options.Create(new MainAppApiOptions { ApiBaseUrl = BaseUrl, PlatformAdminApiKey = key });

    public static IHttpContextAccessor Accessor(ClaimsPrincipal? user = null) =>
        new HttpContextAccessor { HttpContext = new DefaultHttpContext { User = user ?? new ClaimsPrincipal(new ClaimsIdentity()) } };

    public static ClaimsPrincipal SignedIn(AdminIdentity admin) => AdminAuthService.ToPrincipal(admin);

    public static ClaimsPrincipal SignedIn(string id, string email, string name) =>
        new(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, id), new Claim(ClaimTypes.Email, email), new Claim(ClaimTypes.Name, name)
        }, CookieAuthenticationDefaults.AuthenticationScheme));

    public static MemoryCache Cache() => new(new MemoryCacheOptions());

    public static AdminAuthService AuthService(IAuthApiClient api, IMemoryCache? cache = null) =>
        new(api, cache ?? Cache(), NullLogger<AdminAuthService>.Instance);
}
