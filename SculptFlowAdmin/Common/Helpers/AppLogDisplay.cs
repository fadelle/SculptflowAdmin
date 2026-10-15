using Microsoft.AspNetCore.Html;

namespace SculptFlowAdmin.Common.Helpers;

/// <summary>How the Errors pages show a saved log row's level and logger name.</summary>
public static class AppLogDisplay
{
    /// <summary>A chip for "Critical", "Error" or "Warning".</summary>
    public static IHtmlContent Level(string level) => new HtmlString(level switch
    {
        "Critical" => "<span class=\"au-chip au-chip--error\">Critical</span>",
        "Error" => "<span class=\"au-chip au-chip--error au-chip--soft\">Error</span>",
        _ => "<span class=\"au-chip au-chip--warning au-chip--soft\">Warning</span>"
    });

    /// <summary>The last two parts of a dotted name: "PlasticSurgery.Business.HttpClients.N8n.AiTriggerNotifier" → "N8n.AiTriggerNotifier".</summary>
    public static string Short(string name)
    {
        var parts = name.Split('.');
        return parts.Length <= 2 ? name : string.Join('.', parts[^2..]);
    }
}