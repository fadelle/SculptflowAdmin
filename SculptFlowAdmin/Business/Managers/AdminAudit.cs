using System.Text.Json;
using SculptFlowAdmin.Business.Contracts.Managers;
using SculptFlowAdmin.Common.Statics;
using SculptFlowAdmin.Entities.Models;
using SculptFlowAdmin.Persistence.Contracts;
using SculptFlowAdmin.Persistence.Contracts.Admins;

namespace SculptFlowAdmin.Business.Managers;

/// <summary>
/// Records every change an admin makes (admin_audit_log). Every write in Services/ goes through Log, so the
/// Audit log page answers "who changed this, and when".
/// </summary>
public class AdminAudit : IAdminAudit
{
    private readonly IAdminAuditRepository _auditLog;
    private readonly IAdminUnitOfWork _adminUnitOfWork;
    private readonly IHttpContextAccessor _http;

    public AdminAudit(IAdminAuditRepository auditLog, IAdminUnitOfWork adminUnitOfWork, IHttpContextAccessor http)
    {
        _auditLog = auditLog;
        _adminUnitOfWork = adminUnitOfWork;
        _http = http;
    }

    public async Task LogAsync(string action, string entityType, object? entityId, Guid? clinicId = null,
        object? details = null, CancellationToken ct = default)
    {
        var user = _http.HttpContext?.User;
        _auditLog.Add(new AdminAuditEntry
        {
            Id = Guid.NewGuid(),
            AdminUserId = user is null ? null : AdminClaims.Id(user),
            AdminEmail = user is null ? "system" : AdminClaims.Email(user),
            Action = action,
            EntityType = entityType,
            EntityId = entityId?.ToString(),
            ClinicId = clinicId,
            Details = details is null ? null : details as string ?? JsonSerializer.Serialize(details),
            CreatedAt = DateTimeOffset.UtcNow
        });
        await _adminUnitOfWork.SaveChangesAsync(ct);
    }
}
