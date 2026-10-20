using Microsoft.AspNetCore.Mvc;
using Omni.Admin.Business.Contracts.Services.Clients;
using Omni.Admin.Business.Contracts.Services.Contacts;
using Omni.Admin.Common.Enums;
using Omni.Admin.Entities.Responses.PlatformAdmin;
using Omni.Admin.Pages.Shared;

namespace Omni.Admin.Pages.Conversations;

public class DetailsModel : AdminPageModel
{
    private readonly IContactAdminService _contacts;
    private readonly IClientAdminService _clients;

    public DetailsModel(IContactAdminService contacts, IClientAdminService clients)
    {
        _contacts = contacts;
        _clients = clients;
    }

    [BindProperty(SupportsGet = true)] public Guid Id { get; set; }
    public ConversationDetail Conversation { get; private set; } = null!;
    public ClientDetail? Client { get; private set; }
    public List<MessageDetail> Messages { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        var c = await _contacts.GetConversationAsync(Id, ct);
        if (c is null) return NotFound();
        Conversation = c;
        Client = await _clients.GetAsync(c.ClientId, ct);
        Messages = await _contacts.MessagesAsync(Id, 500, ct);
        return Page();
    }

    public Task<IActionResult> OnPostModeAsync(string mode, CancellationToken ct) =>
        RunAsync(() => _contacts.SetModeAsync(Id, mode, ct), mode == ConversationMode.Ai ? "Returned to AI." : "Handed to member.", new { id = Id });

    public Task<IActionResult> OnPostStatusAsync(string status, CancellationToken ct) =>
        RunAsync(() => _contacts.SetStatusAsync(Id, status, ct), $"Conversation marked {status}.", new { id = Id });
}
