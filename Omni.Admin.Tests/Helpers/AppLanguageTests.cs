using Omni.Admin.Common.Enums;

namespace Omni.Admin.Tests.Helpers;

public class AppLanguageTests
{
    [Theory]
    [InlineData(null, "Business default")]
    [InlineData("", "Business default")]
    [InlineData("ar", "Arabic")]
    [InlineData("FR", "French")]
    public void ForUser_names_the_users_own_language_or_the_business_default(string? code, string expected) =>
        Assert.Equal(expected, AppLanguage.ForUser(code));

    [Theory]
    [InlineData("ar", "Arabic (ar)")]
    [InlineData("en", "English (en)")]
    [InlineData(" FR ", "French (fr)")]
    public void ContactLanguage_names_the_dashboard_languages(string code, string expected) =>
        Assert.Equal(expected, AppLanguage.ContactLanguage(code));

    [Theory]
    [InlineData(null)]
    [InlineData(" ")]
    public void ContactLanguage_is_none_without_a_code(string? code) => Assert.Null(AppLanguage.ContactLanguage(code));

    [Fact]
    public void ContactLanguage_names_another_language_when_the_host_knows_it_else_shows_the_code()
    {
        var shown = AppLanguage.ContactLanguage("es");

        Assert.True(shown is "Spanish (es)" or "es", shown);
    }

    [Fact]
    public void ContactLanguage_shows_an_unknown_code_as_it_is() => Assert.Equal("qq", AppLanguage.ContactLanguage("qq"));
}
