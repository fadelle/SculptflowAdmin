using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using Omni.Admin.Business.HttpClients.MainApp;
using Omni.Admin.Common.Exceptions;
using Omni.Admin.Tests.Support;

namespace Omni.Admin.Tests.HttpClients;

public class AuthApiClientTests
{
    private readonly FakeHttpHandler _http = new();

    private AuthApiClient Client(string? key = TestKit.ApiKey) =>
        new(new HttpClient(_http), TestKit.Options(key), TestKit.Accessor(), NullLogger<AuthApiClient>.Instance);

    [Fact]
    public async Task Login_success_posts_the_typed_email_and_password_with_the_key_and_returns_the_admin()
    {
        _http.Enqueue(HttpStatusCode.OK, """{"id":"ad000000-0000-4000-8000-000000000001","fullName":"Ada Admin","email":"ada@omni.test"}""");

        var admin = await Client().LoginAsync("ada@omni.test", "correct-horse-1", default);

        Assert.Equal(TestKit.Ada, admin);
        var request = Assert.Single(_http.Requests);
        Assert.Equal(HttpMethod.Post, request.Method);
        Assert.Equal($"{TestKit.BaseUrl}/api/platform-admin/auth/login", request.Uri.ToString());
        Assert.Equal(TestKit.ApiKey, request.Headers["X-Platform-Admin-Key"]);
        Assert.Equal("admin-portal", request.Headers["X-Admin-Actor"]);
        using var body = JsonDocument.Parse(request.Body!);
        Assert.Equal("ada@omni.test", body.RootElement.GetProperty("email").GetString());
        Assert.Equal("correct-horse-1", body.RootElement.GetProperty("password").GetString());
    }

    [Fact]
    public async Task Login_refused_by_the_main_app_returns_null()
    {
        _http.Enqueue(HttpStatusCode.Unauthorized, TestKit.SignInRefusedBody);

        Assert.Null(await Client().LoginAsync("ada@omni.test", "wrong-password", default));
    }

    [Fact]
    public async Task Login_with_a_wrong_portal_key_is_a_configuration_error_not_a_refused_sign_in()
    {
        _http.Enqueue(HttpStatusCode.Unauthorized, TestKit.KeyRejectedBody);

        var ex = await Assert.ThrowsAsync<MainAppApiException>(() => Client().LoginAsync("ada@omni.test", "pw", default));
        Assert.Contains("PlatformAdminApiKey", ex.Message);
    }

    [Fact]
    public async Task Login_when_the_main_app_has_the_api_off_throws()
    {
        _http.Enqueue(HttpStatusCode.NotFound);

        var ex = await Assert.ThrowsAsync<MainAppApiException>(() => Client().LoginAsync("ada@omni.test", "pw", default));
        Assert.Contains("off", ex.Message);
    }

    [Fact]
    public async Task Login_when_the_main_app_is_down_throws_a_readable_error()
    {
        _http.EnqueueThrow(new HttpRequestException("connection refused"));

        var ex = await Assert.ThrowsAsync<MainAppApiException>(() => Client().LoginAsync("ada@omni.test", "pw", default));
        Assert.Contains("Couldn't reach the main app", ex.Message);
    }

    [Fact]
    public async Task Login_without_a_configured_key_throws_before_calling()
    {
        await Assert.ThrowsAsync<MainAppApiException>(() => Client(key: null).LoginAsync("ada@omni.test", "pw", default));
        Assert.Empty(_http.Requests);
    }

    [Fact]
    public async Task Login_server_error_throws()
    {
        _http.Enqueue(HttpStatusCode.InternalServerError, """{"error":"boom"}""");

        var ex = await Assert.ThrowsAsync<MainAppApiException>(() => Client().LoginAsync("ada@omni.test", "pw", default));
        Assert.Equal("boom", ex.Message);
    }

    [Fact]
    public async Task GetActiveAdmin_returns_the_admin_while_active_and_escapes_the_id()
    {
        _http.Enqueue(HttpStatusCode.OK, """{"id":"a b","fullName":"Ada Admin","email":"ada@omni.test"}""");

        var admin = await Client().GetActiveAdminAsync("a b", default);

        Assert.Equal("a b", admin!.Id);
        Assert.Equal($"{TestKit.BaseUrl}/api/platform-admin/auth/admins/a%20b", Assert.Single(_http.Requests).Uri.AbsoluteUri);
        Assert.Equal(HttpMethod.Get, _http.Requests[0].Method);
    }

    [Fact]
    public async Task GetActiveAdmin_returns_null_when_the_main_app_says_not_an_active_admin()
    {
        _http.Enqueue(HttpStatusCode.NotFound);

        Assert.Null(await Client().GetActiveAdminAsync(TestKit.Ada.Id, default));
    }

    [Fact]
    public async Task GetActiveAdmin_when_the_main_app_is_down_throws()
    {
        _http.EnqueueThrow(new TaskCanceledException("timeout"));

        await Assert.ThrowsAsync<MainAppApiException>(() => Client().GetActiveAdminAsync(TestKit.Ada.Id, default));
    }
}
