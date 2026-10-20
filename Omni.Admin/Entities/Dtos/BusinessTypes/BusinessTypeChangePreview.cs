namespace Omni.Admin.Entities.Dtos.BusinessTypes;

/// <summary>(Copy of the main app's Entities/Dtos/BusinessTypes shape.) What changing a client's business type does:
/// contact counts per current stage and the stage each group moves to.</summary>
public record BusinessTypeChangePreview(Guid ClientId, string FromCode, string ToCode, int ContactCount, IReadOnlyList<StageMove> Moves);
