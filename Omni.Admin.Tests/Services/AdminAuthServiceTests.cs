using Moq;
using Omni.Admin.Business.Contracts.HttpClients.MainApp;
using Omni.Admin.Business.Services.Auth;
using Omni.Admin.Common.Exceptions;
using Omni.Admin.Entities.Dtos.Admins;
using Omni.Admin.Tests.Support;

namespace Omni.Admin.Tests.Services;

/// <summary>Sign-in outcomes. The main app answers one 401 for a wrong password, a locked account, an inactive admin and a
/// client or member account, so the portal shows one message for all of them.</summary>
public class AdminAuthServiceTests
{
    private readonly Mock<IAuthApiClient> _api = new(MockBehavior.Strict);

    [Fact]
    public async Task Success_returns_the_admin()
    {
        _api.Setup(a => a.LoginAsync("ada@omni.test", "correct-horse-1", It.IsAny<CancellationToken>())).ReturnsAsync(TestKit.Ada);

        var result = await TestKit.AuthService(_api.Object).SignInAsync("  ada@omni.test ", "correct-horse-1");

        Assert.Equal(TestKit.Ada, result.Admin);
        Assert.Null(result.Error);
    }

    [Theory]
    [InlineData("wrong password")]
    [InlineData("locked account")]
    [InlineData("inactive admin")]
    [InlineData("client or member account")]
    public async Task Every_refusal_shows_the_same_generic_message(string _)
    {
        _api.Setup(a => a.LoginAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((AdminIdentity?)null);

        var result = await TestKit.AuthService(_api.Object).SignInAsync("someone@omni.test", "whatever-pw");

        Assert.Null(result.Admin);
        Assert.Equal(AdminAuthService.SignInFailed, result.Error);
    }

    [Theory]
    [InlineData(null, "pw")]
    [InlineData("  ", "pw")]
    [InlineData("ada@omni.test", "")]
    [InlineData("ada@omni.test", null)]
    public async Task Blank_email_or_password_is_refused_without_asking_the_main_app(string? email, string? password)
    {
        var result = await TestKit.AuthService(_api.Object).SignInAsync(email, password);

        Assert.Equal(AdminAuthService.SignInFailed, result.Error);
        _api.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Main_app_down_shows_the_unavailable_message_inline()
    {
        _api.Setup(a => a.LoginAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new MainAppApiException("Couldn't reach the main app at https://main.test. Try again in a moment."));

        var result = await TestKit.AuthService(_api.Object).SignInAsync("ada@omni.test", "pw");

        Assert.Null(result.Admin);
        Assert.Equal(AdminAuthService.SignInUnavailable, result.Error);
        Assert.DoesNotContain("main.test", result.Error); // no addresses on the anonymous sign-in page
    }

    [Fact]
    public async Task A_sign_in_is_remembered_so_the_first_page_does_not_ask_again()
    {
        _api.Setup(a => a.LoginAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(TestKit.Ada);
        var auth = TestKit.AuthService(_api.Object);

        await auth.SignInAsync("ada@omni.test", "pw");
        var admin = await auth.GetActiveAdminAsync(TestKit.Ada.Id);

        Assert.Equal(TestKit.Ada, admin);
        _api.Verify(a => a.GetActiveAdminAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task An_active_answer_is_remembered_and_forget_asks_again()
    {
        _api.Setup(a => a.GetActiveAdminAsync(TestKit.Ada.Id, It.IsAny<CancellationToken>())).ReturnsAsync(TestKit.Ada);
        var auth = TestKit.AuthService(_api.Object);

        await auth.GetActiveAdminAsync(TestKit.Ada.Id);
        await auth.GetActiveAdminAsync(TestKit.Ada.Id);
        _api.Verify(a => a.GetActiveAdminAsync(TestKit.Ada.Id, It.IsAny<CancellationToken>()), Times.Once);

        auth.Forget(TestKit.Ada.Id);
        await auth.GetActiveAdminAsync(TestKit.Ada.Id);
        _api.Verify(a => a.GetActiveAdminAsync(TestKit.Ada.Id, It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task A_refusal_is_not_remembered()
    {
        _api.Setup(a => a.GetActiveAdminAsync("gone", It.IsAny<CancellationToken>())).ReturnsAsync((AdminIdentity?)null);
        var auth = TestKit.AuthService(_api.Object);

        Assert.Null(await auth.GetActiveAdminAsync("gone"));
        Assert.Null(await auth.GetActiveAdminAsync("gone"));
        _api.Verify(a => a.GetActiveAdminAsync("gone", It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public void The_remembered_answer_lasts_at_most_a_minute() =>
        Assert.True(AdminAuthService.RecheckAfter <= TimeSpan.FromMinutes(1));

    [Fact]
    public void The_cookie_carries_id_email_and_name_falling_back_to_the_email()
    {
        var principal = AdminAuthService.ToPrincipal(new AdminIdentity("id-1", null, "ada@omni.test"));

        Assert.Equal("id-1", Omni.Admin.Common.Statics.AdminClaims.Id(principal));
        Assert.Equal("ada@omni.test", Omni.Admin.Common.Statics.AdminClaims.Email(principal));
        Assert.Equal("ada@omni.test", Omni.Admin.Common.Statics.AdminClaims.Name(principal));
        Assert.True(principal.Identity!.IsAuthenticated);
    }
}
