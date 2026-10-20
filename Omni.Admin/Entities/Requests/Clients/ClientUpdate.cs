namespace Omni.Admin.Entities.Requests.Clients;

public record ClientUpdate(string Name, string? Phone, string? Email, string? Website, string? CountryCode, string Timezone,
    string? Address, string? OperatingHours, string? BookingInfo);
