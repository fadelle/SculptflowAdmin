using Microsoft.AspNetCore.Mvc;
using Omni.Admin.Business.Contracts.Services.BusinessTypes;
using Omni.Admin.Business.Contracts.Services.Contacts;
using Omni.Admin.Common.Enums;
using Omni.Admin.Entities.Dtos.Contacts;
using Omni.Admin.Entities.Responses.Paging;
using Omni.Admin.Pages.Shared;

namespace Omni.Admin.Pages.Contacts;

[ScopeCapability(ScopeCapability.Client)]
public class IndexModel : AdminPageModel
{
    private readonly IContactAdminService _contacts;
    private readonly IBusinessTypeAdminService _types;
    private readonly ScopeContext _scope;

    public IndexModel(IContactAdminService contacts, IBusinessTypeAdminService types, ScopeContext scope)
    {
        _contacts = contacts;
        _types = types;
        _scope = scope;
    }

    [BindProperty(SupportsGet = true)] public Guid? ClientId { get; set; }
    [BindProperty(SupportsGet = true)] public string? Stage { get; set; }
    [BindProperty(SupportsGet = true)] public string? Q { get; set; }
    [BindProperty(SupportsGet = true, Name = "p")] public int PageNumber { get; set; } = 1;
    public PagedResult<ContactRow> Result { get; private set; } = null!;

    /// <summary>Every stage key of every business type, one entry per key (English label).</summary>
    public IReadOnlyList<(string Key, string Label)> StageOptions { get; private set; } = [];

    public async Task OnGetAsync(CancellationToken ct)
    {
        ClientId ??= _scope.FilterGuid; // the header's client scope when the URL names none
        Result = await _contacts.ListContactsAsync(ClientId, Stage, Q, PageNumber, ct);
        StageOptions = (await _types.ListAsync(ct))
            .SelectMany(t => t.Stages.Select(s => (s.Key, Label: s.Labels.GetValueOrDefault("en") ?? s.Key)))
            .GroupBy(s => s.Key)
            .Select(g => (g.Key, Label: g.First().Label))
            .OrderBy(s => s.Label)
            .ToList();
    }
}
