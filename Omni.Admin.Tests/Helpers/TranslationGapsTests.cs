using Omni.Admin.Common.Enums;
using Omni.Admin.Common.Helpers;
using Omni.Admin.Entities.Dtos.BusinessTypes;
using Omni.Admin.Entities.Responses.BusinessTypes;

namespace Omni.Admin.Tests.Helpers;

public class TranslationGapsTests
{
    private static IReadOnlyDictionary<string, TermForms> Words(string one, string other) =>
        TermName.All.ToDictionary(t => t, _ => new TermForms(one, other));

    private static BusinessTypeResponse Type(
        Dictionary<string, string>? name = null,
        Dictionary<string, IReadOnlyDictionary<string, TermForms>>? terms = null,
        params PipelineStageResponse[] stages) =>
        new("clinic",
            name ?? new() { ["en"] = "Clinic", ["ar"] = "عيادة", ["fr"] = "Clinique" },
            terms ?? new() { ["en"] = Words("patient", "patients"), ["ar"] = Words("مريض", "مرضى"), ["fr"] = Words("patient", "patients") },
            "", true, 1,
            stages.Length > 0 ? stages : [new("new", "open", new Dictionary<string, string> { ["en"] = "New", ["ar"] = "جديد", ["fr"] = "Nouveau" }, 1)],
            0, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow);

    [Fact]
    public void A_fully_translated_type_has_no_gaps() => Assert.Empty(TranslationGaps.Of(Type()));

    [Fact]
    public void A_type_with_only_English_misses_Arabic_and_French_everywhere()
    {
        var type = Type(new() { ["en"] = "Gym" }, new() { ["en"] = Words("member", "members") },
            new PipelineStageResponse("new", "open", new Dictionary<string, string> { ["en"] = "New" }, 1),
            new PipelineStageResponse("won", "won", new Dictionary<string, string> { ["en"] = "Won" }, 2));

        var gaps = TranslationGaps.Of(type);

        Assert.Equal([new TranslationGap("ar", true, TermName.All.Count, 2), new TranslationGap("fr", true, TermName.All.Count, 2)], gaps);
        Assert.Equal("missing Arabic", TranslationGaps.Badge(gaps[0]));
        Assert.Equal($"French: name, {TermName.All.Count} words, 2 stage labels", TranslationGaps.Describe(gaps[1]));
    }

    [Fact]
    public void A_blank_form_of_one_word_or_one_blank_stage_label_counts_for_that_language_only()
    {
        var french = Words("patient", "patients").ToDictionary(p => p.Key, p => p.Value);
        french[TermName.Booking] = new TermForms("rendez-vous", " ");
        var type = Type(terms: new() { ["en"] = Words("patient", "patients"), ["ar"] = Words("مريض", "مرضى"), ["fr"] = french },
            stages: new PipelineStageResponse("new", "open", new Dictionary<string, string> { ["en"] = "New", ["ar"] = "", ["fr"] = "Nouveau" }, 1));

        var gaps = TranslationGaps.Of(type);

        Assert.Equal([new TranslationGap("ar", false, 0, 1), new TranslationGap("fr", false, 1, 0)], gaps);
        Assert.Equal("Arabic: 1 stage label", TranslationGaps.Describe(gaps[0]));
        Assert.Equal("French: 1 word", TranslationGaps.Describe(gaps[1]));
    }
}
