namespace SculptFlowAdmin.Entities.Requests.Clinics;

public record ClinicUpdate(string Name, string? Phone, string? Email, string? Website, string? CountryCode, string Timezone,
    string? Address, string? OperatingHours, string? ConsultationInfo);
