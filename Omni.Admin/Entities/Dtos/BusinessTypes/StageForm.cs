namespace Omni.Admin.Entities.Dtos.BusinessTypes;

/// <summary>One stage row of the business type form (the portal's own). A row with no key and no labels is ignored; a stage is
/// removed by clearing its key. Order only sorts the rows; the saved order is the position after sorting.</summary>
public class StageForm
{
    public string? Key { get; set; }
    public string? Category { get; set; }
    public int Order { get; set; }
    public Dictionary<string, string?> Labels { get; set; } = new();
}
