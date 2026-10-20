namespace Omni.Admin.Common.Enums;

/// <summary>Languages business type names, terminology and stage labels can be written in — a copy of the main app's
/// AppLanguage (two-letter codes; English is required, the others fall back to it).</summary>
public static class AppLanguage
{
    public const string English = "en";
    public const string Arabic = "ar";
    public const string French = "fr";

    public static readonly IReadOnlyList<string> All = [English, Arabic, French];

    public static string Label(string code) => code switch { English => "English", Arabic => "Arabic", French => "French", _ => code };
}
