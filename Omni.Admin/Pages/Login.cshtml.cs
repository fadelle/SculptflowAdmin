using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Omni.Admin.Business.Contracts.Managers;
using Omni.Admin.Business.Contracts.Services.Auth;
using Omni.Admin.Business.Services.Auth;

namespace Omni.Admin.Pages;

[AllowAnonymous]
public class LoginModel : PageModel
{
    private readonly IAdminAuthService _auth;
    private readonly IAdminAudit _audit;

    public LoginModel(IAdminAuthService auth, IAdminAudit audit)
    {
        _auth = auth;
        _audit = audit;
    }

    [BindProperty] public string Email { get; set; } = string.Empty;
    [BindProperty] public string Password { get; set; } = string.Empty;
    [BindProperty(SupportsGet = true)] public string? ReturnUrl { get; set; }
    public string? Error { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync(CancellationToken ct)
    {
        var result = await _auth.SignInAsync(Email, Password, ct);
        if (result.Admin is null)
        {
            Error = result.Error;
            return Page();
        }

        var principal = AdminAuthService.ToPrincipal(result.Admin);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        HttpContext.User = principal;
        await _audit.LogAsync("admin.signed_in", "admin_user", result.Admin.Id, ct: ct);
        return LocalRedirect(Url.IsLocalUrl(ReturnUrl) ? ReturnUrl! : "/");
    }
}
