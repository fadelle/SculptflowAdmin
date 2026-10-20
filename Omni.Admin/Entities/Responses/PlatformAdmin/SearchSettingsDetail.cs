namespace Omni.Admin.Entities.Responses.PlatformAdmin;

/// <summary>(Copy of the main app's Entities/Dtos/PlatformAdmin shape.) A client's knowledge search settings.</summary>
public record SearchSettingsDetail(Guid Id, Guid ClientId, string EmbeddingModel, int VectorDimension, string VectorIndexType,
    int ChunkSizeTokens, int ChunkOverlapTokens, int TopK, double MinimumSimilarity);
