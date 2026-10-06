namespace SculptFlowAdmin.Common.Helpers;

public static class InputText
{
    /// <summary>Trimmed text, or null for blank input.</summary>
    public static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
