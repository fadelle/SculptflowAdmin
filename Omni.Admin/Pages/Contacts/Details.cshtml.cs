using System.Text;
using Microsoft.AspNetCore.Mvc;
using Omni.Admin.Business.Contracts.Services.BusinessTypes;
using Omni.Admin.Business.Contracts.Services.Clients;
using Omni.Admin.Business.Contracts.Services.Contacts;
using Omni.Admin.Business.Services.Contacts;
using Omni.Admin.Common.Exceptions;
using Omni.Admin.Entities.Dtos.Contacts;
using Omni.Admin.Entities.Responses.PlatformAdmin;
using Omni.Admin.Entities.Requests.Contacts;
using Omni.Admin.Pages.Shared;

namespace Omni.Admin.Pages.Contacts;

public class DetailsModel : AdminPageModel
{
    private readonly IContactAdminService _contacts;
    private readonly IClientAdminService _clients;
    private readonly IBusinessTypeAdminService _types;

    public DetailsModel(IContactAdminService contacts, IClientAdminService clients, IBusinessTypeAdminService types)
    {
        _contacts = contacts;
        _clients = clients;
        _types = types;
    }

    [BindProperty(SupportsGet = true)] public Guid Id { get; set; }
    public ContactDetail Contact { get; private set; } = null!;
    public IReadOnlyList<ConversationRow> Conversations { get; private set; } = [];
    public IReadOnlyList<BookingRow> Bookings { get; private set; } = [];
    public List<ContactEventRow> Events { get; private set; } = new();

    /// <summary>The stages of the contact's client's business type, in pipeline order, as (key, English label).</summary>
    public IReadOnlyList<(string Key, string Label)> Stages { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        var contact = await _contacts.GetContactAsync(Id, ct);
        if (contact is null) return NotFound();
        Contact = contact;
        Conversations = (await _contacts.ListConversationsAsync(null, Id, null, null, null, 1, ct)).Items;
        Bookings = (await _contacts.ListBookingsAsync(null, Id, null, false, 1, ct)).Items;
        Events = await _contacts.ContactEventsAsync(Id, ct);
        var client = await _clients.GetAsync(contact.ClientId, ct);
        var type = client is null ? null : await _types.GetAsync(client.BusinessTypeCode, ct);
        Stages = type?.Stages.OrderBy(s => s.SortOrder).Select(s => (s.Key, s.Labels.GetValueOrDefault("en") ?? s.Key)).ToList() ?? [];
        return Page();
    }

    /// <summary>What the admin types to confirm a delete (the contact's name, or "delete").</summary>
    public string DeleteWord => ContactAdminService.ConfirmationWord(Contact);

    /// <summary>Downloads everything the main app holds about the contact (privacy request).</summary>
    public async Task<IActionResult> OnGetExportAsync(CancellationToken ct)
    {
        var json = await _contacts.ExportContactAsync(Id, ct);
        return json is null ? NotFound() : File(Encoding.UTF8.GetBytes(json), "application/json", $"contact-{Id}.json");
    }

    /// <summary>Deletes the contact and all its data for good, then goes to the client's contact list.</summary>
    public async Task<IActionResult> OnPostDeleteAsync(string? confirmation, CancellationToken ct)
    {
        try
        {
            var clientId = await _contacts.DeleteContactAsync(Id, confirmation, ct);
            Flash = "The contact and all its data were deleted.";
            return RedirectToPage("/Contacts/Index", new { clientId });
        }
        catch (ArgumentException ex) { FlashError = ex.Message; }
        catch (AdminRuleException ex) { FlashError = ex.Message; }
        catch (KeyNotFoundException) { FlashError = "That contact no longer exists."; }
        return RedirectToPage(new { id = Id });
    }

    public Task<IActionResult> OnPostUpdateAsync(string stageKey, string qualification, bool marketingOptIn, string? notes,
        CancellationToken ct) =>
        RunAsync(() => _contacts.UpdateContactAsync(Id, new ContactUpdate(stageKey, qualification, marketingOptIn, notes), ct),
            "Contact saved.", new { id = Id });
}
