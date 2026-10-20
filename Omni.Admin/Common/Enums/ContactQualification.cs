namespace Omni.Admin.Common.Enums;

/// <summary>Allowed values for a contact's qualification (how promising it looks; the same for every business type) — a
/// copy of the main app's ContactQualification.</summary>
public static class ContactQualification
{
    public const string Unknown = "unknown";
    public const string Hot = "hot";
    public const string Warm = "warm";
    public const string Cold = "cold";
    public const string NeedsHuman = "needs_human";
    public const string Spam = "spam";

    public static readonly IReadOnlySet<string> All = new HashSet<string>
    {
        Unknown, Hot, Warm, Cold, NeedsHuman, Spam
    };
}
