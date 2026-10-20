using Microsoft.AspNetCore.Mvc;
using Omni.Admin.Business.Contracts.Services.Contacts;
using Omni.Admin.Common.Enums;
using Omni.Admin.Entities.Dtos.Contacts;
using Omni.Admin.Entities.Models;
using Omni.Admin.Entities.Responses.Paging;
using Omni.Admin.Pages.Shared;

namespace Omni.Admin.Pages.Bookings;

[ScopeCapability(ScopeCapability.Client)]
public class IndexModel : AdminPageModel
{
    private readonly IContactAdminService _contacts;
    private readonly ScopeContext _scope;

    public IndexModel(IContactAdminService contacts, ScopeContext scope)
    {
        _contacts = contacts;
        _scope = scope;
    }

    [BindProperty(SupportsGet = true)] public Guid? ClientId { get; set; }
    [BindProperty(SupportsGet = true)] public string? Status { get; set; }
    [BindProperty(SupportsGet = true)] public bool Upcoming { get; set; }
    [BindProperty(SupportsGet = true, Name = "p")] public int PageNumber { get; set; } = 1;
    public PagedResult<BookingRow> Result { get; private set; } = null!;

    public async Task OnGetAsync(CancellationToken ct)
    {
        ClientId ??= _scope.FilterGuid; // the header's client scope when the URL names none
        Result = await _contacts.ListBookingsAsync(ClientId, null, Status, Upcoming, PageNumber, ct);
    }

    /// <summary>Posted from the shared bookings table (also embedded on contact pages); goes back where it came from.</summary>
    public async Task<IActionResult> OnPostStatusAsync(Guid id, string status, string? returnUrl, CancellationToken ct)
    {
        await RunAsync(() => _contacts.SetBookingStatusAsync(id, status, ct), $"Booking marked {status.Replace('_', ' ')}.");
        return LocalRedirect(Url.IsLocalUrl(returnUrl) ? returnUrl! : "/Bookings");
    }
}
