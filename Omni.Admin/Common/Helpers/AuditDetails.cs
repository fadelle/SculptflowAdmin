using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Omni.Admin.Common.Helpers;

/// <summary>
/// Turns an audit row's stored details (JSON written by AdminAudit) into short lines a person reads (QA t6-admin-10: raw JSON
/// with \u escapes wrapped a few characters per line). <c>{before, after}</c> becomes "Field: old → new" for each field that
/// changed (fields recorded on one side only are left out: older rows recorded fewer "before" fields); <c>{from, to}</c>
/// becomes "old → new"; any other object one "Field: value" line per property; text that isn't JSON stays as it is.
/// </summary>
public static class AuditDetails
{
    /// <summary>The JSON options AdminAudit writes with: Arabic, French and "+" stay readable instead of \u escapes.</summary>
    public static readonly JsonSerializerOptions Json = new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    public const int MaxValueLength = 120;

    /// <summary>One line: an optional field label and its text. A change also carries its two sides (Old, New) so the page can
    /// isolate each one: an Arabic value must not flip the order of "old → new".</summary>
    public record Line(string? Label, string Text, string? Old = null, string? New = null)
    {
        public static Line Change(string? label, string old, string @new) => new(label, $"{old} → {@new}", old, @new);
    }

    /// <summary>One line as an &lt;li&gt;: "Label: value", or "Label: old → new" with each side in a &lt;bdi&gt; inside a
    /// left-to-right span, so an Arabic value can't reorder the arrow. Everything is HTML-encoded.</summary>
    public static IHtmlContent Html(Line line)
    {
        var enc = HtmlEncoder.Default;
        var label = line.Label is null ? "" : $"<span class=\"au-audit-label\">{enc.Encode(line.Label)}:</span>";
        var text = line.Old is not null && line.New is not null
            ? $"<span dir=\"ltr\"><bdi>{enc.Encode(line.Old)}</bdi> → <bdi>{enc.Encode(line.New)}</bdi></span>"
            : $"<span dir=\"auto\">{enc.Encode(line.Text)}</span>";
        return new HtmlString($"<li>{label}{text}</li>");
    }

    public static IReadOnlyList<Line> Describe(string? details)
    {
        if (string.IsNullOrWhiteSpace(details)) return [];
        JsonNode? root;
        try { root = JsonNode.Parse(details); }
        catch (JsonException) { return [new Line(null, Clip(details))]; }

        if (root is not JsonObject obj) return [new Line(null, Value(root))];
        if (Get(obj, "before") is JsonObject before && Get(obj, "after") is JsonObject after) return Diff(before, after);
        if (obj.Count == 2 && Key(obj, "from") is { } from && Key(obj, "to") is { } to)
        {
            return [Line.Change(null, Value(obj[from]), Value(obj[to]))];
        }
        return obj.Select(p => new Line(Label(p.Key), Value(p.Value))).ToList();
    }

    private static IReadOnlyList<Line> Diff(JsonObject before, JsonObject after)
    {
        var lines = new List<Line>();
        foreach (var (key, newValue) in after)
        {
            if (Key(before, key) is not { } beforeKey) continue;
            var oldValue = before[beforeKey];
            if (Same(oldValue, newValue)) continue;
            lines.Add(Line.Change(Label(key), Value(oldValue), Value(newValue)));
        }
        return lines.Count > 0 ? lines : [new Line(null, "No field changed")];
    }

    /// <summary>Equal values; null, "" and whitespace count as the same "nothing", and text is compared trimmed.</summary>
    private static bool Same(JsonNode? a, JsonNode? b) => Normal(a) == Normal(b);

    private static string? Normal(JsonNode? node) => node switch
    {
        null => null,
        JsonValue v when v.TryGetValue<string>(out var s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim(),
        _ => node.ToJsonString(Json)
    };

    private static string Value(JsonNode? node) => node switch
    {
        null => "—",
        JsonValue v when v.TryGetValue<string>(out var s) => string.IsNullOrWhiteSpace(s) ? "—" : Clip(s.Trim()),
        JsonValue v when v.TryGetValue<bool>(out var b) => b ? "yes" : "no",
        _ => Clip(node.ToJsonString(Json))
    };

    private static string Clip(string text) => text.Length <= MaxValueLength ? text : text[..(MaxValueLength - 1)] + "…";

    private static JsonNode? Get(JsonObject obj, string name) => Key(obj, name) is { } key ? obj[key] : null;

    /// <summary>The property's actual name, matched without case ("Name" and "name" are the same field).</summary>
    private static string? Key(JsonObject obj, string name) =>
        obj.Select(p => p.Key).FirstOrDefault(k => string.Equals(k, name, StringComparison.OrdinalIgnoreCase));

    /// <summary>"CountryCode" / "countryCode" / "country_code" → "Country code".</summary>
    public static string Label(string key)
    {
        var words = new List<string>();
        var current = new System.Text.StringBuilder();
        foreach (var c in key)
        {
            if (c is '_' or '-' or ' ' || (char.IsUpper(c) && current.Length > 0 && !char.IsUpper(current[^1])))
            {
                if (current.Length > 0) words.Add(current.ToString());
                current.Clear();
                if (c is '_' or '-' or ' ') continue;
            }
            current.Append(c);
        }
        if (current.Length > 0) words.Add(current.ToString());
        if (words.Count == 0) return key;
        var text = string.Join(' ', words.Select(w => w.All(char.IsUpper) && w.Length > 1 ? w : w.ToLowerInvariant()));
        return char.ToUpperInvariant(text[0]) + text[1..];
    }
}
