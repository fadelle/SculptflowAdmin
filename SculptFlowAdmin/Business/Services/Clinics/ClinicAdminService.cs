using SculptFlowAdmin.Business.Contracts.Managers;
using SculptFlowAdmin.Business.Contracts.Services.Clinics;
using SculptFlowAdmin.Common.Exceptions;
using SculptFlowAdmin.Common.Helpers;
using SculptFlowAdmin.Entities.Dtos.Clinics;
using SculptFlowAdmin.Entities.Models;
using SculptFlowAdmin.Entities.Requests.Clinics;
using SculptFlowAdmin.Entities.Responses.Paging;
using SculptFlowAdmin.Persistence.Contracts;
using SculptFlowAdmin.Persistence.Contracts.Clinics;

namespace SculptFlowAdmin.Business.Services.Clinics;

public class ClinicAdminService : IClinicAdminService
{
    private readonly IClinicAdminRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAdminAudit _audit;

    public ClinicAdminService(IClinicAdminRepository repository, IUnitOfWork unitOfWork, IAdminAudit audit)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _audit = audit;
    }

    public Task<PagedResult<ClinicRow>> ListAsync(string? search, bool? active, int page, CancellationToken ct = default) =>
        _repository.ListAsync(search, active, page, ct);

    public Task<List<ClinicOption>> OptionsAsync(CancellationToken ct = default) =>
        _repository.OptionsAsync(ct);

    public Task<Clinic?> GetAsync(Guid id, CancellationToken ct = default) =>
        _repository.GetAsync(id, ct);

    public Task<ClinicCounts> CountsAsync(Guid id, CancellationToken ct = default) =>
        _repository.CountsAsync(id, ct);

    public async Task UpdateAsync(Guid id, ClinicUpdate update, CancellationToken ct = default)
    {
        var clinic = await _repository.GetForUpdateAsync(id, ct) ?? throw new KeyNotFoundException();
        if (string.IsNullOrWhiteSpace(update.Name)) throw new AdminRuleException("Clinic name is required.");
        var tz = string.IsNullOrWhiteSpace(update.Timezone) ? "UTC" : update.Timezone.Trim();
        if (!TimeZoneInfo.TryFindSystemTimeZoneById(tz, out _)) throw new AdminRuleException($"Unknown time zone '{tz}'. Use an IANA name such as Asia/Beirut.");

        var before = new { clinic.Name, clinic.Phone, clinic.Email, clinic.Website, clinic.CountryCode, clinic.Timezone };
        clinic.Name = update.Name.Trim();
        clinic.Phone = InputText.Clean(update.Phone);
        clinic.Email = InputText.Clean(update.Email);
        clinic.Website = InputText.Clean(update.Website);
        clinic.CountryCode = InputText.Clean(update.CountryCode)?.ToUpperInvariant();
        clinic.Timezone = tz;
        clinic.Address = InputText.Clean(update.Address);
        clinic.OperatingHours = InputText.Clean(update.OperatingHours);
        clinic.ConsultationInfo = InputText.Clean(update.ConsultationInfo);
        clinic.UpdatedAt = DateTimeOffset.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);
        await _audit.LogAsync("clinic.updated", "clinic", id, id, new { before, after = update }, ct);
    }

    public async Task SetActiveAsync(Guid id, bool active, CancellationToken ct = default)
    {
        var clinic = await _repository.GetForUpdateAsync(id, ct) ?? throw new KeyNotFoundException();
        if (clinic.IsActive == active) return;
        clinic.IsActive = active;
        clinic.UpdatedAt = DateTimeOffset.UtcNow;
        await _unitOfWork.SaveChangesAsync(ct);
        await _audit.LogAsync(active ? "clinic.activated" : "clinic.deactivated", "clinic", id, id, ct: ct);
    }

}
