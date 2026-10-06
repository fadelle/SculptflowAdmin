using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SculptFlowAdmin.Common.Exceptions;

namespace SculptFlowAdmin.Pages.Shared;

/// <summary>Base for admin pages: a one-line result message after a POST, and AdminRuleException shown as an error.</summary>
public abstract class AdminPageModel : PageModel
{
    [TempData] public string? Flash { get; set; }
    [TempData] public string? FlashError { get; set; }

    /// <summary>Runs a write, then redirects back to the same page (PRG) with the outcome as a flash message.</summary>
    protected async Task<IActionResult> RunAsync(Func<Task> action, string success, object? routeValues = null)
    {
        try
        {
            await action();
            Flash = success;
        }
        catch (AdminRuleException ex) { FlashError = ex.Message; }
        catch (ArgumentException ex) { FlashError = ex.Message; }
        catch (KeyNotFoundException) { FlashError = "That record no longer exists."; }
        return RedirectToPage(null, routeValues ?? RouteValuesFromQuery());
    }

    private object RouteValuesFromQuery() =>
        Request.Query.ToDictionary(k => k.Key, v => (object?)v.Value.ToString());
}
