using Omni.Admin.Entities.Dtos.BusinessTypes;

namespace Omni.Admin.Entities.Requests.BusinessTypes;

/// <summary>(Copy of the main app's Entities/Requests/BusinessTypes shape.) Create (POST, Code required) or replace (PUT,
/// Code ignored) a business type. Name and Terminology are per language ("en" required and complete); Stages is the whole
/// pipeline in order.</summary>
public record SaveBusinessTypeRequest(
    string? Code,
    Dictionary<string, string>? Name,
    Dictionary<string, Dictionary<string, TermForms>>? Terminology,
    string? AiProfile,
    int SortOrder,
    List<SaveStageRequest>? Stages);
