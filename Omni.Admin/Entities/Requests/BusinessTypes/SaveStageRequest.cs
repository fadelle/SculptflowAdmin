namespace Omni.Admin.Entities.Requests.BusinessTypes;

/// <summary>(Copy of the main app's Entities/Requests/BusinessTypes shape.) One stage; its position in the list is its order.</summary>
public record SaveStageRequest(string? Key, string? Category, Dictionary<string, string>? Labels);
