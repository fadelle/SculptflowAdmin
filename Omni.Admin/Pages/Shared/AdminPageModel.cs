using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Omni.Admin.Common.Exceptions;

using Omni.Admin.Pages;

namespace Omni.Admin.Pages.Shared;

/// <summary>Base for admin pages: a one-line result message after a POST, and AdminRuleException shown as an error.</summary>
public abstract class AdminPageModel : PageModel
{
    [TempData] public string? Flash { get; set; }
    [TempData] public string? FlashError { get; set; }

    /// <summary>
    /// Every page reads through the main app's platform-admin API. When a GET can't (main app down, key wrong), show the
    /// reason on the Unavailable page instead of an error page. Pages that load with MainAppPageModel.LoadAsync handle it
    /// themselves and never get here.
    /// </summary>
    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var executed = await next();
        if (executed.Exception is MainAppApiException ex && !executed.ExceptionHandled && HttpMethods.IsGet(Request.Method))
        {
            executed.ExceptionHandled = true;
            // The reason travels in TempData, not the URL, so a link can't put its own text in the error panel (QA t6-admin-17).
            TempData[UnavailableModel.ReasonKey] = ex.Message;
            executed.Result = RedirectToPage("/Unavailable", new { from = Request.Path + Request.QueryString });
        }
    }

    /// <summary>Runs a write, then redirects back to the same page (PRG) with the outcome as a flash message.</summary>
    protected Task<IActionResult> RunAsync(Func<Task> action, string success, object? routeValues = null) =>
        RunAsync(action, () => success, routeValues);

    /// <summary>As above, with a success message that depends on what the write answered.</summary>
    protected async Task<IActionResult> RunAsync(Func<Task> action, Func<string> success, object? routeValues = null)
    {
        try
        {
            await action();
            Flash = success();
        }
        catch (AdminRuleException ex) { FlashError = ex.Message; }
        catch (ArgumentException ex) { FlashError = ex.Message; }
        catch (KeyNotFoundException) { FlashError = "That record no longer exists."; }
        return RedirectToPage(null, routeValues ?? RouteValuesFromQuery());
    }

    private object RouteValuesFromQuery() =>
        Request.Query.ToDictionary(k => k.Key, v => (object?)v.Value.ToString());
}
