using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using Omni.Admin.Business.HttpClients.MainApp;
using Omni.Admin.Tests.Support;

namespace Omni.Admin.Tests.HttpClients;

public class MembersApiClientTests
{
    private readonly FakeHttpHandler _http = new();

    private MembersApiClient Client() =>
        new(new HttpClient(_http), TestKit.Options(), TestKit.Accessor(TestKit.SignedIn(TestKit.Ada)), NullLogger<MembersApiClient>.Instance);

    private static string Row(string id, string language) => $$"""
        {"userId":"{{id}}","email":"{{id}}@omni.test","fullName":null,"membershipId":null,"clientId":null,"clientName":null,
         "membershipActive":true,"loginLocked":false,"emailConfirmed":true,"accessFailedCount":0,"hasPassword":true,
         "hasGoogleLogin":false,"joinedAt":null,"role":"member","userActive":true,"lastLoginAt":null,"language":{{language}}}
        """;

    [Fact]
    public async Task List_reads_each_users_own_language_or_none()
    {
        _http.Enqueue(HttpStatusCode.OK, $$"""{"items":[{{Row("u1", "\"ar\"")}},{{Row("u2", "null")}}],"page":1,"pageSize":50,"total":2}""");

        var result = await Client().ListAsync(null, null, 1, default);

        Assert.Equal("ar", result.Items[0].Language);
        Assert.Null(result.Items[1].Language);
    }
}
