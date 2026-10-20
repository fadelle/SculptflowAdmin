namespace Omni.Admin.Entities.Dtos.BusinessTypes;

/// <summary>(Copy of the main app's Entities/Dtos/BusinessTypes shape.) One word of a business type's terminology in one
/// language: singular ("one") and plural ("other"), written as they read mid-sentence.</summary>
public record TermForms(string One, string Other);
