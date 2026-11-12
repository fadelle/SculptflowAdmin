using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Omni.Admin.Pages;

/// <summary>Shown when a page couldn't read from the main app's platform-admin API (see AdminPageModel).</summary>
public class UnavailableModel : PageModel
{
    /// <summary>The TempData key the reason travels in (never the URL: QA t6-admin-17, any text in ?reason= showed as the error).</summary>
    public const string ReasonKey = "UnavailableReason";

    public string Reason { get; private set; } = "The main app didn't answer.";
    public string? From { get; private set; }

    public void OnGet(string? from)
    {
        if (TempData[ReasonKey] is string reason && !string.IsNullOrWhiteSpace(reason)) Reason = reason;
        // Only a local path, so the "Try again" link can't point anywhere else.
        From = from is not null && from.StartsWith('/') && !from.StartsWith("//") && !from.StartsWith("/Unavailable") ? from : null;
    }
}
