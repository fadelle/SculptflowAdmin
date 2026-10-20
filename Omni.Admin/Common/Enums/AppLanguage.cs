using System.Globalization;

namespace Omni.Admin.Common.Enums;

/// <summary>Languages business type names, terminology and stage labels can be written in — a copy of the main app's
/// AppLanguage (two-letter codes; English is required, the others fall back to it). The same three are the dashboard
/// languages a client or user can pick.</summary>
public static class AppLanguage
{
    public const string English = "en";
    public const string Arabic = "ar";
    public const string French = "fr";

    public static readonly IReadOnlyList<string> All = [English, Arabic, French];

    /// <summary>The languages besides English that business types should be translated into.</summary>
    public static readonly IReadOnlyList<string> Translations = [Arabic, French];

    public static string Label(string code) => code switch { English => "English", Arabic => "Arabic", French => "French", _ => code };

    /// <summary>A user's own dashboard language; none means they follow the client's default.</summary>
    public static string ForUser(string? code) =>
        string.IsNullOrWhiteSpace(code) ? "Business default" : Label(code.Trim().ToLowerInvariant());

    /// <summary>A contact's language (stored by the main app as a two-letter ISO 639-1 code) by its English name with the
    /// code, e.g. "Spanish (es)"; just the code when the name isn't known here, null when there is none.</summary>
    public static string? ContactLanguage(string? code)
    {
        if (string.IsNullOrWhiteSpace(code)) return null;
        code = code.Trim().ToLowerInvariant();
        var name = Label(code);
        if (name == code)
        {
            try
            {
                var english = CultureInfo.GetCultureInfo(code).EnglishName;
                if (!string.IsNullOrWhiteSpace(english) && english != code && !english.StartsWith("Unknown", StringComparison.OrdinalIgnoreCase))
                    name = english;
            }
            catch (CultureNotFoundException) { }
        }
        return name == code ? code : $"{name} ({code})";
    }
}
