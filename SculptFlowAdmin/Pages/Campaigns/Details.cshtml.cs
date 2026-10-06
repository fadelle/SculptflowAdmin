using Microsoft.AspNetCore.Mvc;
using SculptFlowAdmin.Business.Contracts.Services.Content;
using SculptFlowAdmin.Entities.Dtos.Content;
using SculptFlowAdmin.Entities.Models;
using SculptFlowAdmin.Entities.Responses.Paging;
using SculptFlowAdmin.Pages.Shared;

namespace SculptFlowAdmin.Pages.Campaigns;

public class DetailsModel : AdminPageModel
{
    private readonly IContentAdminService _content;

    public DetailsModel(IContentAdminService content) => _content = content;

    [BindProperty(SupportsGet = true)] public Guid Id { get; set; }
    [BindProperty(SupportsGet = true)] public string? Status { get; set; }
    [BindProperty(SupportsGet = true, Name = "p")] public int PageNumber { get; set; } = 1;
    public CampaignRow Campaign { get; private set; } = null!;
    public PagedResult<RecipientRow> Recipients { get; private set; } = null!;

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        var campaign = await _content.GetCampaignAsync(Id, ct);
        if (campaign is null) return NotFound();
        Campaign = campaign;
        Recipients = await _content.RecipientsAsync(Id, Status, PageNumber, ct);
        return Page();
    }

    public Task<IActionResult> OnPostCancelAsync(CancellationToken ct) =>
        RunAsync(() => _content.CancelCampaignAsync(Id, ct), "Campaign cancelled. Messages already sent are unaffected.", new { id = Id });
}
