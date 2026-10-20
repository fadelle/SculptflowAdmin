using Omni.Admin.Entities.Dtos.BusinessTypes;

namespace Omni.Admin.Entities.Responses.BusinessTypes;

/// <summary>(Copy of the main app's Entities/Responses/BusinessTypes shape.) A business type: every language it has, its
/// stages in pipeline order, and how many clients use it.</summary>
public record BusinessTypeResponse(
    string Code,
    IReadOnlyDictionary<string, string> Name,
    IReadOnlyDictionary<string, IReadOnlyDictionary<string, TermForms>> Terminology,
    string AiProfile,
    bool IsActive,
    int SortOrder,
    IReadOnlyList<PipelineStageResponse> Stages,
    int ClientCount,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
