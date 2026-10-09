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
        "error", "failed", "rejected", "problem", "no_show", "lost", "spam", "locked", "critical", "flagged"
    };
    /// <summary>Switched off / ended, not broken (docs/UI_GUIDE.md §9.3: inactive, disabled, archived → neutral).</summary>
    private static readonly HashSet<string> Off = new(StringComparer.OrdinalIgnoreCase)
    {
        "disabled", "inactive", "no", "expired", "cancelled"
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

    /// <summary>The ONE status → tone map (docs/UI_GUIDE.md §9.3), as an Aurora chip: healthy = success, waiting = warning,
    /// broken = error, informational = outlined info, AI = outlined primary, anything else (incl. switched off) = neutral.</summary>
    public static IHtmlContent Badge(string? value, string? label = null)
    {
        if (string.IsNullOrWhiteSpace(value)) return new HtmlString("<span class=\"au-faint\">—</span>");
        var tone = Green.Contains(value) ? "au-chip--success" : Red.Contains(value) ? "au-chip--error"
            : Amber.Contains(value) ? "au-chip--warning" : Off.Contains(value) ? "au-chip--outlined"
            : Blue.Contains(value) ? "au-chip--outlined au-chip--info" : Purple.Contains(value) ? "au-chip--outlined au-chip--primary"
            : "au-chip--outlined";
        var text = HtmlEncoder.Default.Encode(label ?? value.Replace('_', ' '));
        return new HtmlString($"<span class=\"au-chip {tone}\">{text}</span>");
    }

    public static IHtmlContent YesNo(bool value, string yes = "Yes", string no = "No") => Badge(value ? "yes" : "no", value ? yes : no);

    /// <summary>A UTC timestamp, shown in the viewer's time zone by site.js (or in <paramref name="timeZone"/> when given).</summary>
    public static IHtmlContent Time(DateTimeOffset? value, string? timeZone = null)
    {
        if (value is null) return new HtmlString("<span class=\"au-faint\">—</span>");
        var iso = value.Value.ToUniversalTime().ToString("o");
        var tz = string.IsNullOrEmpty(timeZone) ? "" : $" data-tz=\"{HtmlEncoder.Default.Encode(timeZone)}\"";
        return new HtmlString($"<time class=\"nowrap\" data-utc=\"{iso}\"{tz}>{value.Value.UtcDateTime:yyyy-MM-dd HH:mm} UTC</time>");
    }

    /// <summary>The text, or a faint em dash when there is none (docs/UI_GUIDE.md §10: never blank).</summary>
    public static IHtmlContent OrDash(string? text) =>
        string.IsNullOrWhiteSpace(text) ? new HtmlString("<span class=\"au-faint\">—</span>") : new HtmlString(HtmlEncoder.Default.Encode(text));

    /// <summary>"1 lead" / "1,284 leads".</summary>
    public static string Count(long n, string one, string many) => $"{n:N0} {(n == 1 ? one : many)}";

    public static string Truncate(string? text, int max) =>
        string.IsNullOrEmpty(text) ? "" : text.Length <= max ? text : text[..max] + "…";

    public static string Short(Guid id) => id.ToString()[..8];

    /// <summary>Up to two initials for an avatar: "Mona Darwish" → "MD".</summary>
    public static string Initials(string name) =>
        string.Concat(name.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(2).Select(p => char.ToUpperInvariant(p[0])));
}
