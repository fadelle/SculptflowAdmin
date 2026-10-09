using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SculptFlowAdmin.Pages;

/// <summary>
/// The page shown for an error status with no body (404, 400…), re-executed by UseStatusCodePagesWithReExecute in
/// Program.cs (not for /api). Anonymous and antiforgery-free so it also renders for signed-out visitors and failed
/// POSTs.
/// </summary>
[AllowAnonymous]
[IgnoreAntiforgeryToken]
[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
public class StatusModel : PageModel
{
    public int Code { get; private set; }
    public string? OriginalPath { get; private set; }

    public void OnGet(int code) => Load(code);

    public void OnPost(int code) => Load(code);

    private void Load(int code)
    {
        Code = code is >= 400 and <= 599 ? code : 404;
        OriginalPath = HttpContext.Features.Get<IStatusCodeReExecuteFeature>()?.OriginalPath;
    }
}
