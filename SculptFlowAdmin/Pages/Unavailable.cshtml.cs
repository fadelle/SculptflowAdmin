using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SculptFlowAdmin.Pages;

/// <summary>Shown when a page couldn't read from the main app's platform-admin API (see AdminPageModel).</summary>
public class UnavailableModel : PageModel
{
    public string Reason { get; private set; } = "The main app didn't answer.";
    public string? From { get; private set; }

    public void OnGet(string? reason, string? from)
    {
        if (!string.IsNullOrWhiteSpace(reason)) Reason = reason;
        // Only a local path, so the "Try again" link can't point anywhere else.
        From = from is not null && from.StartsWith('/') && !from.StartsWith("//") && !from.StartsWith("/Unavailable") ? from : null;
    }
}
