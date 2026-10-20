namespace Omni.Admin.Entities.Dtos.BusinessTypes;

/// <summary>(Copy of the main app's Entities/Dtos/BusinessTypes shape.) Where the contacts in one stage go when a client
/// changes business type. FromLabel is the old type's label (or the key itself when it has no such stage).</summary>
public record StageMove(string FromKey, string FromLabel, string Category, int Count, string ToKey, string ToLabel);
