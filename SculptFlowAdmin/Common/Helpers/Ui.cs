using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SculptFlowAdmin.Common.Helpers;

/// <summary>Small rendering helpers shared by every page.</summary>
public static class Ui
{
    private static readonly HashSet<string> Green = new(StringComparer.OrdinalIgnoreCase)
    {
        "connected", "active", "approved", "healthy", "completed", "delivered", "read", "replied", "booked", "confirmed",
        "attended", "indexed", "synced", "settled", "billable", "hot", "qualified", "consultation_booked", "consultation_attended", "surgery_booked", "yes"
    };
    private static readonly HashSet<string> Red = new(StringComparer.OrdinalIgnoreCase)
    {
        "error", "failed", "rejected", "problem", "disabled", "no_show", "lost", "spam", "locked", "inactive", "critical", "flagged", "no", "expired", "cancelled"
    };
    private static readonly HashSet<string> Amber = new(StringComparer.OrdinalIgnoreCase)
    {
        "pending", "warning", "paused", "running", "scheduled", "queued", "human", "needs_human", "medical_question", "warm",
        "crawling", "completed_with_errors", "rescheduled", "approval", "past_due", "reserved"
    };
    private static readonly HashSet<string> Blue = new(StringComparer.OrdinalIgnoreCase)
    {
        "new", "sent", "contacted", "draft", "infobip", "meta", "telegram", "cold"
    };
    private static readonly HashSet<string> Purple = new(StringComparer.OrdinalIgnoreCase) { "ai" };

    public static IHtmlContent Badge(string? value, string? label = null)
    {
        if (string.IsNullOrWhiteSpace(value)) return new HtmlString("<span class=\"muted\">—</span>");
        var cls = Green.Contains(value) ? "green" : Red.Contains(value) ? "red" : Amber.Contains(value) ? "amber"
            : Blue.Contains(value) ? "blue" : Purple.Contains(value) ? "purple" : "";
        var text = HtmlEncoder.Default.Encode(label ?? value.Replace('_', ' '));
        return new HtmlString($"<span class=\"badge {cls}\">{text}</span>");
    }

    public static IHtmlContent YesNo(bool value, string yes = "Yes", string no = "No") => Badge(value ? "yes" : "no", value ? yes : no);

    /// <summary>A UTC timestamp, shown in the viewer's time zone by site.js (or in <paramref name="timeZone"/> when given).</summary>
    public static IHtmlContent Time(DateTimeOffset? value, string? timeZone = null)
    {
        if (value is null) return new HtmlString("<span class=\"muted\">—</span>");
        var iso = value.Value.ToUniversalTime().ToString("o");
        var tz = string.IsNullOrEmpty(timeZone) ? "" : $" data-tz=\"{HtmlEncoder.Default.Encode(timeZone)}\"";
        return new HtmlString($"<time class=\"nowrap\" data-utc=\"{iso}\"{tz}>{value.Value.UtcDateTime:yyyy-MM-dd HH:mm} UTC</time>");
    }

    public static string Truncate(string? text, int max) =>
        string.IsNullOrEmpty(text) ? "" : text.Length <= max ? text : text[..max] + "…";

    public static string Short(Guid id) => id.ToString()[..8];
}
