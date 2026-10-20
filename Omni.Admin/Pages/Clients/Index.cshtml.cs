using Microsoft.AspNetCore.Mvc;
using Omni.Admin.Business.Contracts.Services.Clients;
using Omni.Admin.Entities.Dtos.Clients;
using Omni.Admin.Entities.Responses.Paging;
using Omni.Admin.Pages.Shared;

namespace Omni.Admin.Pages.Clients;

public class IndexModel : AdminPageModel
{
    private readonly IClientAdminService _clients;

    public IndexModel(IClientAdminService clients) => _clients = clients;

    [BindProperty(SupportsGet = true)] public string? Q { get; set; }
    [BindProperty(SupportsGet = true)] public string? Active { get; set; }
    [BindProperty(SupportsGet = true, Name = "p")] public int PageNumber { get; set; } = 1;
    public PagedResult<ClientRow> Result { get; private set; } = null!;

    public async Task OnGetAsync(CancellationToken ct)
    {
        bool? active = Active switch { "true" => true, "false" => false, _ => null };
        Result = await _clients.ListAsync(Q, active, PageNumber, ct);
    }
}
