using Omni.Admin.Common.Enums;
using Omni.Admin.Common.Exceptions;
using Omni.Admin.Entities.Dtos.BusinessTypes;
using Omni.Admin.Entities.Requests.BusinessTypes;
using Omni.Admin.Entities.Responses.BusinessTypes;

namespace Omni.Admin.Business.Mappers.BusinessTypes;

/// <summary>Between the business type edit form (flat text boxes) and the main app's save request / response.</summary>
public static class BusinessTypeFormMapper
{
    /// <summary>Extra blank stage rows shown under the existing ones, for adding stages.</summary>
    public const int SpareStageRows = 4;

    public static string TermKey(string language, string term) => $"{language}|{term}";

    /// <summary>The form for a type; <paramref name="forNewCopy"/> keeps its content but clears the code (a new type that starts as a copy).</summary>
    public static BusinessTypeForm FromResponse(BusinessTypeResponse type, bool forNewCopy = false)
    {
        var form = new BusinessTypeForm
        {
            Code = forNewCopy ? null : type.Code,
            SortOrder = type.SortOrder,
            AiProfile = type.AiProfile,
            Name = type.Name.ToDictionary(p => p.Key, p => (string?)p.Value),
        };
        foreach (var (language, terms) in type.Terminology)
        {
            foreach (var (term, forms) in terms)
            {
                form.TermOne[TermKey(language, term)] = forms.One;
                form.TermOther[TermKey(language, term)] = forms.Other;
            }
        }
        var order = 0;
        foreach (var stage in type.Stages.OrderBy(s => s.SortOrder))
        {
            form.Stages.Add(new StageForm
            {
                Key = stage.Key,
                Category = stage.Category,
                Order = ++order,
                Labels = stage.Labels.ToDictionary(p => p.Key, p => (string?)p.Value),
            });
        }
        return form;
    }

    /// <summary>The same form with blank rows added for new stages.</summary>
    public static BusinessTypeForm WithSpareRows(BusinessTypeForm form)
    {
        var next = form.Stages.Count == 0 ? 0 : form.Stages.Max(s => s.Order);
        for (var i = 0; i < SpareStageRows; i++) form.Stages.Add(new StageForm { Order = ++next });
        return form;
    }

    /// <summary>The save request for the form. Blank text is left out; a word with only one of its two forms, or a stage
    /// with labels but no key, is refused here (the main app would silently drop them).</summary>
    public static SaveBusinessTypeRequest ToRequest(BusinessTypeForm form)
    {
        var name = Texts(form.Name);

        var terminology = new Dictionary<string, Dictionary<string, TermForms>>();
        foreach (var language in AppLanguage.All)
        {
            var terms = new Dictionary<string, TermForms>();
            foreach (var term in TermName.All)
            {
                var key = TermKey(language, term);
                var one = Clean(form.TermOne.GetValueOrDefault(key));
                var other = Clean(form.TermOther.GetValueOrDefault(key));
                if (one is null && other is null) continue;
                if (one is null || other is null)
                    throw new AdminRuleException($"The word '{term}' in {AppLanguage.Label(language)} needs both a singular and a plural form (or leave both empty).");
                terms[term] = new TermForms(one, other);
            }
            if (terms.Count > 0) terminology[language] = terms;
        }

        var stages = new List<SaveStageRequest>();
        var rows = form.Stages.Select((s, i) => (Stage: s, Index: i)).OrderBy(r => r.Stage.Order).ThenBy(r => r.Index);
        foreach (var (stage, _) in rows)
        {
            var key = Clean(stage.Key);
            var labels = Texts(stage.Labels);
            if (key is null)
            {
                if (labels.Count > 0) throw new AdminRuleException("A stage with a label needs a key. Clear its labels to leave the row out.");
                continue;
            }
            stages.Add(new SaveStageRequest(key, Clean(stage.Category), labels));
        }

        return new SaveBusinessTypeRequest(Clean(form.Code), name, terminology, form.AiProfile?.Trim() ?? string.Empty, form.SortOrder, stages);
    }

    private static Dictionary<string, string> Texts(Dictionary<string, string?>? raw) =>
        (raw ?? new()).Where(p => Clean(p.Value) is not null).ToDictionary(p => p.Key, p => Clean(p.Value)!);

    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
