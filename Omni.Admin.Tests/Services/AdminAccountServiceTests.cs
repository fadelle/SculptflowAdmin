using Moq;
using Omni.Admin.Business.Contracts.HttpClients.MainApp;
using Omni.Admin.Business.Contracts.Managers;
using Omni.Admin.Business.Contracts.Services.Auth;
using Omni.Admin.Business.Services.Admins;
using Omni.Admin.Common.Exceptions;
using Omni.Admin.Entities.Dtos.Admins;

namespace Omni.Admin.Tests.Services;

public class AdminAccountServiceTests
{
    private static readonly AdminRow Row = new("ad2", "bob@omni.test", "Bob Admin", true, false, null, DateTimeOffset.UtcNow);

    private readonly Mock<IAdminsApiClient> _api = new(MockBehavior.Strict);
    private readonly Mock<IAdminAuthService> _auth = new();
    private readonly Mock<IAdminAudit> _audit = new();

    private AdminAccountService Service() => new(_api.Object, _auth.Object, _audit.Object);

    private void VerifyAudited(string action, string id) =>
        _audit.Verify(a => a.LogAsync(action, "admin_user", id, null, It.IsAny<object?>(), It.IsAny<CancellationToken>()), Times.Once);

    [Fact]
    public async Task List_comes_from_the_main_app()
    {
        _api.Setup(a => a.ListAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new[] { Row });

        Assert.Equal(Row, Assert.Single(await Service().ListAsync()));
    }

    [Fact]
    public async Task Create_trims_the_inputs_and_audits_under_the_new_admins_id()
    {
        _api.Setup(a => a.CreateAsync("bob@omni.test", "Bob Admin", " pw with spaces ", It.IsAny<CancellationToken>())).ReturnsAsync(Row);

        await Service().CreateAsync(" bob@omni.test ", " Bob Admin ", " pw with spaces ");

        VerifyAudited("admin.created", "ad2");
    }

    [Fact]
    public async Task Deactivate_forgets_the_remembered_answer_and_audits()
    {
        _api.Setup(a => a.SetActiveAsync("ad2", false, "ad1", It.IsAny<CancellationToken>())).ReturnsAsync(Row with { IsActive = false });

        await Service().SetActiveAsync("ad2", false, "ad1");

        _auth.Verify(a => a.Forget("ad2"), Times.Once);
        VerifyAudited("admin.deactivated", "ad2");
    }

    [Fact]
    public async Task Activate_audits_without_forgetting()
    {
        _api.Setup(a => a.SetActiveAsync("ad2", true, "ad1", It.IsAny<CancellationToken>())).ReturnsAsync(Row);

        await Service().SetActiveAsync("ad2", true, "ad1");

        _auth.Verify(a => a.Forget(It.IsAny<string>()), Times.Never);
        VerifyAudited("admin.activated", "ad2");
    }

    [Fact]
    public async Task Set_password_audits()
    {
        _api.Setup(a => a.SetPasswordAsync("ad2", "new-long-password", It.IsAny<CancellationToken>())).ReturnsAsync(Row);

        await Service().SetPasswordAsync("ad2", "new-long-password");

        VerifyAudited("admin.password_set", "ad2");
    }

    [Fact]
    public async Task A_refused_change_is_not_audited()
    {
        _api.Setup(a => a.SetActiveAsync("ad1", false, "ad1", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new MainAppApiException("You can't deactivate your own account."));

        await Assert.ThrowsAsync<MainAppApiException>(() => Service().SetActiveAsync("ad1", false, "ad1"));

        _audit.VerifyNoOtherCalls();
        _auth.VerifyNoOtherCalls();
    }
}
