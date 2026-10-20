using Omni.Admin.Common.Enums;
using Omni.Admin.Entities.Dtos.BusinessTypes;
using Omni.Admin.Entities.Responses.BusinessTypes;

namespace Omni.Admin.Common.Helpers;

/// <summary>Which translations (Arabic, French) a business type is missing, computed from the main app's response: the
/// name, any word of <see cref="TermName.All"/> without both forms, or any stage label left empty in that language.</summary>
public static class TranslationGaps
{
    public static IReadOnlyList<TranslationGap> Of(BusinessTypeResponse type)
    {
        var gaps = new List<TranslationGap>();
        foreach (var language in AppLanguage.Translations)
        {
            var name = string.IsNullOrWhiteSpace(type.Name.GetValueOrDefault(language));
            var words = type.Terminology.GetValueOrDefault(language);
            var terms = TermName.All.Count(term => words?.GetValueOrDefault(term) is not { } forms
                || string.IsNullOrWhiteSpace(forms.One) || string.IsNullOrWhiteSpace(forms.Other));
            var stages = type.Stages.Count(s => string.IsNullOrWhiteSpace(s.Labels.GetValueOrDefault(language)));
            if (name || terms > 0 || stages > 0) gaps.Add(new TranslationGap(language, name, terms, stages));
        }
        return gaps;
    }

    /// <summary>"missing Arabic"</summary>
    public static string Badge(TranslationGap gap) => $"missing {AppLanguage.Label(gap.Language)}";

    /// <summary>"Arabic: name, 2 words, 3 stage labels"</summary>
    public static string Describe(TranslationGap gap)
    {
        var parts = new List<string>();
        if (gap.Name) parts.Add("name");
        if (gap.Terms > 0) parts.Add(Ui.Count(gap.Terms, "word", "words"));
        if (gap.StageLabels > 0) parts.Add(Ui.Count(gap.StageLabels, "stage label", "stage labels"));
        return $"{AppLanguage.Label(gap.Language)}: {string.Join(", ", parts)}";
    }
}
