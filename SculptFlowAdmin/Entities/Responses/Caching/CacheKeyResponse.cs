namespace SculptFlowAdmin.Entities.Responses.Caching;

/// <summary>One cached key in the main app (copy of its CacheEntryInfo).</summary>
public record CacheKeyResponse(string Key, DateTimeOffset ExpiresAt, int SizeBytes);
