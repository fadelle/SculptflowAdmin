using Microsoft.AspNetCore.Mvc;
using Omni.Admin.Business.Contracts.Services.BusinessTypes;
using Omni.Admin.Entities.Responses.BusinessTypes;
using Omni.Admin.Pages.Shared;

namespace Omni.Admin.Pages.BusinessTypes;

public class IndexModel : AdminPageModel
{
    private readonly IBusinessTypeAdminService _types;

    public IndexModel(IBusinessTypeAdminService types) => _types = types;

    public List<BusinessTypeResponse> Types { get; private set; } = new();

    public async Task OnGetAsync(CancellationToken ct) => Types = await _types.ListAsync(ct);

    public Task<IActionResult> OnPostSetActiveAsync(string code, bool active, CancellationToken ct) =>
        RunAsync(() => _types.SetActiveAsync(code, active, ct), active ? "Business type activated." : "Business type deactivated.");
}
