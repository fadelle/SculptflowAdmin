using System.Text.Json;

namespace Omni.Admin.Entities.Responses.Caching;

/// <summary>One cached key with its stored value (copy of the main app's CacheEntryResponse).</summary>
public record CacheEntryResponse(string Key, DateTimeOffset ExpiresAt, int SizeBytes, JsonElement Value);
