using Moq;
using Omni.Admin.Business.Managers;
using Omni.Admin.Entities.Models;
using Omni.Admin.Persistence.Contracts;
using Omni.Admin.Persistence.Contracts.Admins;
using Omni.Admin.Tests.Support;

namespace Omni.Admin.Tests.Managers;

public class AdminAuditTests
{
    private readonly Mock<IAdminAuditRepository> _log = new();
    private readonly Mock<IAdminUnitOfWork> _unitOfWork = new();
    private AdminAuditEntry? _added;

    public AdminAuditTests() => _log.Setup(l => l.Add(It.IsAny<AdminAuditEntry>())).Callback<AdminAuditEntry>(e => _added = e);

    [Fact]
    public async Task An_entry_is_filed_under_the_signed_in_admins_user_id_as_text()
    {
        var clientId = Guid.NewGuid();
        var audit = new AdminAudit(_log.Object, _unitOfWork.Object, TestKit.Accessor(TestKit.SignedIn(TestKit.Ada)));

        await audit.LogAsync("client.updated", "client", clientId, clientId, new { Name = "x" });

        Assert.NotNull(_added);
        Assert.Equal(TestKit.Ada.Id, _added!.UserId);
        Assert.Equal("ada@omni.test", _added.AdminEmail);
        Assert.Equal(clientId, _added.ClientId);
        Assert.Equal(clientId.ToString(), _added.EntityId);
        Assert.Equal("""{"Name":"x"}""", _added.Details);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Without_a_signed_in_admin_the_user_id_is_empty()
    {
        var audit = new AdminAudit(_log.Object, _unitOfWork.Object, TestKit.Accessor());

        await audit.LogAsync("admin.signed_in", "admin_user", "x");

        Assert.Null(_added!.UserId);
        Assert.Equal("unknown", _added.AdminEmail);
    }
}
