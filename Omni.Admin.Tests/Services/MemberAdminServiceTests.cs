using Moq;
using Omni.Admin.Business.Contracts.HttpClients.MainApp;
using Omni.Admin.Business.Contracts.Managers;
using Omni.Admin.Business.Services.Members;
using Omni.Admin.Entities.Responses.Members;
using Omni.Admin.Entities.Responses.PlatformAdmin;

namespace Omni.Admin.Tests.Services;

/// <summary>Milestone C P1: the reset email and invite cancel go through the main app and are audited under its client.</summary>
public class MemberAdminServiceTests
{
    private readonly Mock<IMembersApiClient> _api = new();
    private readonly Mock<IAdminAudit> _audit = new();
    private readonly Guid _clientId = Guid.NewGuid();

    private MemberAdminService Sut() => new(_api.Object, _audit.Object);

    [Fact]
    public async Task A_reset_email_is_sent_by_the_main_app_and_audited()
    {
        _api.Setup(a => a.SendPasswordResetEmailAsync("u1", It.IsAny<CancellationToken>())).ReturnsAsync(new PlatformAdminChange(_clientId));
        await Sut().SendPasswordResetEmailAsync("u1");
        _audit.Verify(a => a.LogAsync("member.password_reset_email_sent", "identity_user", "u1", _clientId, It.IsAny<object?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Invites_are_read_and_cancelled_through_the_main_app()
    {
        var inviteId = Guid.NewGuid();
        _api.Setup(a => a.ListInvitesAsync(_clientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([new InviteResponse(inviteId, "ann@x.com", null, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddDays(7), false)]);
        _api.Setup(a => a.RevokeInviteAsync(inviteId, It.IsAny<CancellationToken>())).ReturnsAsync(new PlatformAdminChange(_clientId));

        Assert.Single(await Sut().ListInvitesAsync(_clientId));
        await Sut().RevokeInviteAsync(inviteId);
        _audit.Verify(a => a.LogAsync("member.invite_revoked", "member_invite", inviteId.ToString(), _clientId, It.IsAny<object?>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
