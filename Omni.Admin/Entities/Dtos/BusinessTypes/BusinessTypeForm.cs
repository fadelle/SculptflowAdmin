namespace Omni.Admin.Entities.Dtos.BusinessTypes;

/// <summary>The business type edit form (the portal's own). TermOne / TermOther are keyed "language|term", e.g. "en|contact".</summary>
public class BusinessTypeForm
{
    public string? Code { get; set; }
    public int SortOrder { get; set; }
    public string? AiProfile { get; set; }
    public Dictionary<string, string?> Name { get; set; } = new();
    public Dictionary<string, string?> TermOne { get; set; } = new();
    public Dictionary<string, string?> TermOther { get; set; } = new();
    public List<StageForm> Stages { get; set; } = new();
}
