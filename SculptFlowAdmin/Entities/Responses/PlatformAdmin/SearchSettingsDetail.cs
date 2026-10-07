namespace SculptFlowAdmin.Entities.Responses.PlatformAdmin;

/// <summary>(Copy of the main app's Entities/Dtos/PlatformAdmin shape.) A clinic's knowledge search settings.</summary>
public record SearchSettingsDetail(Guid Id, Guid ClinicId, string EmbeddingModel, int VectorDimension, string VectorIndexType,
    int ChunkSizeTokens, int ChunkOverlapTokens, int TopK, double MinimumSimilarity);
