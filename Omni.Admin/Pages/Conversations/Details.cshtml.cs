using Microsoft.AspNetCore.Mvc;
using Omni.Admin.Business.Contracts.Services.Clients;
using Omni.Admin.Business.Contracts.Services.Contacts;
using Omni.Admin.Business.Contracts.Services.Monitoring;
using Omni.Admin.Common.Exceptions;
using Omni.Admin.Common.Helpers;
using Omni.Admin.Common.Enums;
using Omni.Admin.Entities.Responses.PlatformAdmin;
using Omni.Admin.Pages.Shared;

namespace Omni.Admin.Pages.Conversations;

public class DetailsModel : AdminPageModel
{
    private readonly IContactAdminService _contacts;
    private readonly IClientAdminService _clients;
    private readonly IMessageTracesAdminService _traces;

    public DetailsModel(IContactAdminService contacts, IClientAdminService clients, IMessageTracesAdminService traces)
    {
        _traces = traces;
        _contacts = contacts;
        _clients = clients;
    }

    [BindProperty(SupportsGet = true)] public Guid Id { get; set; }
    public ConversationDetail Conversation { get; private set; } = null!;
    public ClientDetail? Client { get; private set; }
    public List<MessageDetail> Messages { get; private set; } = new();
    /// <summary>What happened to the customer's last message, in plain words (QA t6-admin-28); null when there is none or
    /// the trace couldn't be read.</summary>
    public BotAnswerNote? LastAnswer { get; private set; }
    public MessageDetail? LastCustomerMessage { get; private set; }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        var c = await _contacts.GetConversationAsync(Id, ct);
        if (c is null) return NotFound();
        Conversation = c;
        var client = _clients.GetAsync(c.ClientId, ct);
        var messages = _contacts.MessagesAsync(Id, 500, ct);
        await Task.WhenAll(client, messages);
        Client = client.Result;
        Messages = messages.Result;
        LastCustomerMessage = Messages.Where(m => m.Direction == MessageDirection.Inbound).MaxBy(m => m.CreatedAt);
        if (LastCustomerMessage is { } last)
        {
            try
            {
                LastAnswer = BotAnswerWhy.Explain(await _traces.GetAsync(null, last.Id, ct), c.ClientId);
            }
            catch (MainAppApiException)
            {
                LastAnswer = null; // the page still shows the messages
            }
        }
        return Page();
    }

    public Task<IActionResult> OnPostModeAsync(string mode, CancellationToken ct) =>
        RunAsync(() => _contacts.SetModeAsync(Id, mode, ct), mode == ConversationMode.Ai ? "Returned to AI." : "Handed to member.", new { id = Id });

    public Task<IActionResult> OnPostStatusAsync(string status, CancellationToken ct) =>
        RunAsync(() => _contacts.SetStatusAsync(Id, status, ct), $"Conversation marked {status}.", new { id = Id });
}
