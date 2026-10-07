namespace SculptFlowAdmin.Entities.Responses.Caching;

/// <summary>How many keys a remove-by-prefix or clear-all call removed (copy of the main app's CacheClearResponse).</summary>
public record CacheClearResponse(int Removed);
