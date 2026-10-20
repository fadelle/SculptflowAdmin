namespace Omni.Admin.Entities.Dtos.BusinessTypes;

/// <summary>What a business type is missing in one language: its name, how many of its words (a word counts when either
/// form is empty) and how many stage labels.</summary>
public record TranslationGap(string Language, bool Name, int Terms, int StageLabels);
