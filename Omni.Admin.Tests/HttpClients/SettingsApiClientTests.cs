using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using Omni.Admin.Business.HttpClients.MainApp;
using Omni.Admin.Tests.Support;

namespace Omni.Admin.Tests.HttpClients;

public class SettingsApiClientTests
{
    private readonly FakeHttpHandler _http = new();

    private SettingsApiClient Client() =>
        new(new HttpClient(_http), TestKit.Options(), TestKit.Accessor(TestKit.SignedIn(TestKit.Ada)), NullLogger<SettingsApiClient>.Instance);

    private static string Row(string section, string key, string choices) => $$"""
        {"section":"{{section}}","key":"{{key}}","type":"String","description":"d","defaultValue":"n8n","storedValue":null,
         "effectiveValue":"n8n","min":null,"max":null,"note":null,"updatedBy":null,"updatedAt":null,"isSecret":false,"choices":{{choices}}}
        """;

    [Fact]
    public async Task List_reads_the_choices_of_an_n8n_or_code_switch_and_none_for_free_text()
    {
        _http.Enqueue(HttpStatusCode.OK, $$"""[{{Row("Ai", "BotEngine", "[\"n8n\",\"code\"]")}},{{Row("Ai", "BotModel", "null")}}]""");

        var settings = await Client().ListAsync(default);

        Assert.Equal(["n8n", "code"], settings[0].Choices);
        Assert.Null(settings[1].Choices);
        Assert.EndsWith("api/platform-admin/settings/", Assert.Single(_http.Requests).Uri.AbsoluteUri);
    }
}
