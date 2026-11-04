using System.Text.Json;
using Omni.Admin.Common.Helpers;

namespace Omni.Admin.Tests.Helpers;

/// <summary>The audit log's Details column (QA t6-admin-10): readable "what changed" lines instead of raw, escaped JSON.</summary>
public class AuditDetailsTests
{
    private static string Text(AuditDetails.Line line) => line.Label is null ? line.Text : $"{line.Label}: {line.Text}";

    private static List<string> Lines(string? details) => AuditDetails.Describe(details).Select(Text).ToList();

    [Fact]
    public void Before_and_after_show_only_the_fields_that_changed()
    {
        var json = JsonSerializer.Serialize(new
        {
            before = new { Name = "Glow", Phone = "+961 1 555", Email = (string?)null, CountryCode = "LB", Address = "Hamra" },
            after = new { Name = "Glow Clinic", Phone = "+961 1 555", Email = "", CountryCode = "AE", Address = " Hamra ", Timezone = "Asia/Dubai" }
        });

        // Email null → "" and a trimmed address are not changes; Timezone wasn't recorded before (an older row), so it's left out.
        Assert.Equal(["Name: Glow → Glow Clinic", "Country code: LB → AE"], Lines(json));
    }

    [Fact]
    public void Old_rows_with_escaped_text_read_as_the_text_itself()
    {
        // What QA saw: + for "+" and ع for Arabic letters.
        const string stored = """{"before":{"Name":"عيادة","Phone":"+961"},"after":{"Name":"عيادة نور","Phone":"+961"}}""";
        Assert.Equal(["Name: عيادة → عيادة نور"], Lines(stored));
        // Each side is kept apart so the page can isolate it (an Arabic value must not reorder "old → new").
        var line = Assert.Single(AuditDetails.Describe(stored));
        Assert.Equal(("عيادة", "عيادة نور"), (line.Old, line.New));
    }

    [Fact]
    public void A_save_that_changed_nothing_says_so()
    {
        Assert.Equal(["No field changed"], Lines("""{"before":{"TopK":5},"after":{"topK":5}}"""));
    }

    [Fact]
    public void From_and_to_read_as_one_arrow_line()
    {
        Assert.Equal(["en → ar"], Lines("""{"from":"en","to":"ar"}"""));
        Assert.Equal(["— → beauty"], Lines("""{"from":null,"to":"beauty"}"""));
    }

    [Fact]
    public void Other_objects_list_their_fields_and_booleans_read_as_yes_or_no()
    {
        Assert.Equal(["Amount: 1.00", "Reference: X-1", "Notify client: yes", "Stages: 7"],
            Lines("""{"Amount":1.00,"Reference":"X-1","notifyClient":true,"stages":7}"""));
        Assert.Equal(["To: paused"], Lines("""{"to":"paused"}"""));
    }

    [Fact]
    public void Plain_text_empty_and_long_values()
    {
        Assert.Equal(["Imported by script"], Lines("Imported by script"));
        Assert.Empty(Lines(null));
        Assert.Empty(Lines("  "));
        var longValue = Assert.Single(Lines(JsonSerializer.Serialize(new { Notes = new string('n', 500) })));
        Assert.EndsWith("…", longValue);
        Assert.True(longValue.Length <= "Notes: ".Length + AuditDetails.MaxValueLength);
    }

    [Fact]
    public void A_line_renders_encoded_with_each_side_of_a_change_isolated()
    {
        static string Html(AuditDetails.Line line) => AuditDetails.Html(line).ToString()!;
        var change = Assert.Single(AuditDetails.Describe("""{"before":{"Name":"<b>A</b>"},"after":{"Name":"عيادة"}}"""));
        Assert.Equal("<li><span class=\"au-audit-label\">Name:</span><span dir=\"ltr\"><bdi>&lt;b&gt;A&lt;/b&gt;</bdi> → <bdi>&#x639;&#x64A;&#x627;&#x62F;&#x629;</bdi></span></li>",
            Html(change));
        Assert.Equal("<li><span dir=\"auto\">plain &amp; simple</span></li>", Html(new AuditDetails.Line(null, "plain & simple")));
    }

    [Theory]
    [InlineData("CountryCode", "Country code")]
    [InlineData("countryCode", "Country code")]
    [InlineData("country_code", "Country code")]
    [InlineData("TopK", "Top k")]
    [InlineData("AI", "AI")]
    [InlineData("aiBotNewConversations", "Ai bot new conversations")]
    public void Field_names_read_as_words(string key, string expected) => Assert.Equal(expected, AuditDetails.Label(key));
}
