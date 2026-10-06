using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SculptFlowAdmin.Business.Contracts.Managers;
using SculptFlowAdmin.Business.Contracts.Services.Auth;
using SculptFlowAdmin.Business.Services.Auth;

namespace SculptFlowAdmin.Pages;

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
        var (user, error) = await _auth.ValidateAsync(Email, Password, ct);
        if (user is null)
        {
            Error = error;
            return Page();
        }

        var principal = AdminAuthService.ToPrincipal(user);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        HttpContext.User = principal;
        await _audit.LogAsync("admin.signed_in", "admin_user", user.Id, ct: ct);
        return LocalRedirect(Url.IsLocalUrl(ReturnUrl) ? ReturnUrl! : "/");
    }
}
