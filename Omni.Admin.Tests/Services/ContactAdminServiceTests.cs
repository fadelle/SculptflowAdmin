using Moq;
using Omni.Admin.Business.Contracts.HttpClients.MainApp;
using Omni.Admin.Business.Contracts.Managers;
using Omni.Admin.Business.Services.Contacts;
using Omni.Admin.Entities.Responses.PlatformAdmin;

namespace Omni.Admin.Tests.Services;

/// <summary>Milestone C P2: a contact is deleted only after its name is typed, then audited; exports are audited too.</summary>
public class ContactAdminServiceTests
{
    private readonly Mock<IContactsApiClient> _api = new();
    private readonly Mock<IAdminAudit> _audit = new();
    private readonly Guid _id = Guid.NewGuid();
    private readonly Guid _clientId = Guid.NewGuid();

    private ContactAdminService Sut() => new(_api.Object, _audit.Object);

    private void Contact(string? fullName, string? first = null, string? last = null) =>
        _api.Setup(a => a.GetContactAsync(_id, It.IsAny<CancellationToken>())).ReturnsAsync(new ContactDetail(_id, _clientId, null, null, null,
            fullName, first, last, "+9611000000", null, null, null, null, null, "new", "unqualified", null, null, null, null, null, true, null,
            null, null, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow));

    [Theory]
    [InlineData("Rana Haddad", "  rana   HADDAD ")]
    [InlineData(null, "delete")]
    public async Task The_typed_name_confirms_the_delete(string? name, string typed)
    {
        Contact(name);
        _api.Setup(a => a.DeleteContactAsync(_id, It.IsAny<CancellationToken>())).ReturnsAsync(new PlatformAdminChange(_clientId));

        Assert.Equal(_clientId, await Sut().DeleteContactAsync(_id, typed));

        _audit.Verify(a => a.LogAsync("contact.deleted", "contact", _id, _clientId, It.IsAny<object?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task A_wrong_name_deletes_nothing_and_a_missing_contact_is_not_found()
    {
        Contact(null, "Rana", "Haddad");
        var ex = await Assert.ThrowsAsync<ArgumentException>(() => Sut().DeleteContactAsync(_id, "Rana"));
        Assert.Contains("Rana Haddad", ex.Message);
        _api.Verify(a => a.DeleteContactAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => Sut().DeleteContactAsync(Guid.NewGuid(), "x"));
    }

    [Fact]
    public async Task An_export_is_audited_only_when_the_contact_exists()
    {
        Contact("Rana Haddad");
        _api.Setup(a => a.ExportContactAsync(_id, It.IsAny<CancellationToken>())).ReturnsAsync("{}");
        Assert.Equal("{}", await Sut().ExportContactAsync(_id));
        Assert.Null(await Sut().ExportContactAsync(Guid.NewGuid()));
        _audit.Verify(a => a.LogAsync("contact.exported", "contact", _id, _clientId, null, It.IsAny<CancellationToken>()), Times.Once);
        _api.Verify(a => a.ExportContactAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
