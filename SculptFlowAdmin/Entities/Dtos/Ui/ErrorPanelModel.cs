using System.Diagnostics;

namespace SculptFlowAdmin.Entities.Dtos.Ui;

/// <summary>
/// Everything the _ErrorPanel partial shows: headline · cause · what to check · the failing call · details · copy.
/// Build it with From(); never show a bare "An error occurred".
/// </summary>
public sealed record ErrorPanelModel(string Title, string Message, string? Hint = null, string? Call = null,
    IReadOnlyList<KeyValuePair<string, string>>? Facts = null)
{
    /// <summary>A failure the app already explains in words (e.g. MainAppPageModel.LoadError, the Unavailable page's
    /// reason), with the request and its trace id so it can be reported.</summary>
    public static ErrorPanelModel From(string title, string message, HttpContext http, string? hint = null)
    {
        var traceId = Activity.Current?.Id ?? http.TraceIdentifier;
        var facts = new List<KeyValuePair<string, string>> { new("Trace id", traceId) };
        return new ErrorPanelModel(title, message, hint, $"{http.Request.Method} {http.Request.Path} · trace {traceId}", facts);
    }

    /// <summary>The whole panel as plain text, for the copy button.</summary>
    public string Report => string.Join(Environment.NewLine, new[] { Title, Message, Hint is null ? null : "What to check: " + Hint, Call }
        .Concat((Facts ?? Array.Empty<KeyValuePair<string, string>>()).Select(f => $"{f.Key}: {f.Value}"))
        .Where(s => !string.IsNullOrEmpty(s)));
}
