namespace SculptFlowAdmin.Common.Helpers;

public static class Paging
{
    public const int DefaultPageSize = 50;

    /// <summary>Case-insensitive "contains" pattern for EF.Functions.ILike, with % and _ escaped.</summary>
    public static string Like(string term) =>
        "%" + term.Trim().Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_") + "%";
}
