namespace Omni.Admin.Entities.Responses.BusinessTypes;

/// <summary>(Copy of the main app's Entities/Responses/BusinessTypes shape.)</summary>
public record PipelineStageResponse(string Key, string Category, IReadOnlyDictionary<string, string> Labels, int SortOrder);
