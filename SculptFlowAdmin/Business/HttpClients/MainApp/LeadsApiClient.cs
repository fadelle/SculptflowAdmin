using Microsoft.Extensions.Options;
using SculptFlowAdmin.Business.Contracts.HttpClients.MainApp;
using SculptFlowAdmin.Common.Configs;
using SculptFlowAdmin.Entities.Dtos.Leads;
using SculptFlowAdmin.Entities.Requests.Leads;
using SculptFlowAdmin.Entities.Responses.Paging;
using SculptFlowAdmin.Entities.Responses.PlatformAdmin;

namespace SculptFlowAdmin.Business.HttpClients.MainApp;

/// <summary>Client for the main app's platform-admin leads API (/api/platform-admin/leads): leads, conversations,
/// messages and appointments.</summary>
public class LeadsApiClient : MainAppApiClient, ILeadsApiClient
{
    public const string Prefix = "api/platform-admin/leads";

    public LeadsApiClient(HttpClient http, IOptions<MainAppApiOptions> options, IHttpContextAccessor context, ILogger<LeadsApiClient> logger)
        : base(http, options, context, logger, Prefix, "Leads")
    {
    }

    public Task<PagedResult<LeadRow>> ListLeadsAsync(Guid? clinicId, string? status, string? search, int page, CancellationToken ct) =>
        GetRequiredAsync<PagedResult<LeadRow>>(Query(("clinicId", clinicId), ("status", status), ("search", search), ("page", page)), ct);

    public Task<LeadDetail?> GetLeadAsync(Guid id, CancellationToken ct) => GetAsync<LeadDetail>($"/{id}", ct);

    public Task<List<LeadEventRow>> LeadEventsAsync(Guid id, CancellationToken ct) => GetRequiredAsync<List<LeadEventRow>>($"/{id}/events", ct);

    public Task<PlatformAdminChange> UpdateLeadAsync(Guid id, LeadUpdate update, CancellationToken ct) =>
        WriteForAsync<PlatformAdminChange>(HttpMethod.Put, $"/{id}", update, null, false, ct);

    public Task<PagedResult<ConversationRow>> ListConversationsAsync(Guid? clinicId, Guid? leadId, string? channel, string? mode,
        string? status, int page, CancellationToken ct) =>
        GetRequiredAsync<PagedResult<ConversationRow>>("/conversations" + Query(("clinicId", clinicId), ("leadId", leadId),
            ("channel", channel), ("mode", mode), ("status", status), ("page", page)), ct);

    public Task<ConversationDetail?> GetConversationAsync(Guid id, CancellationToken ct) => GetAsync<ConversationDetail>($"/conversations/{id}", ct);

    public Task<List<MessageDetail>> MessagesAsync(Guid conversationId, int take, CancellationToken ct) =>
        GetRequiredAsync<List<MessageDetail>>($"/conversations/{conversationId}/messages" + Query(("take", take)), ct);

    public Task<PlatformAdminChange> SetModeAsync(Guid id, string mode, CancellationToken ct) =>
        WriteForAsync<PlatformAdminChange>(HttpMethod.Put, $"/conversations/{id}/mode", new ModeBody(mode), null, false, ct);

    public Task<PlatformAdminChange> SetStatusAsync(Guid id, string status, CancellationToken ct) =>
        WriteForAsync<PlatformAdminChange>(HttpMethod.Put, $"/conversations/{id}/status", new StatusBody(status), null, false, ct);

    public Task<PagedResult<MessageRow>> ListMessagesAsync(Guid? clinicId, bool failedOnly, string? sender, string? search, int page,
        CancellationToken ct) =>
        GetRequiredAsync<PagedResult<MessageRow>>("/messages" + Query(("clinicId", clinicId), ("failedOnly", failedOnly),
            ("sender", sender), ("search", search), ("page", page)), ct);

    public Task<PagedResult<AppointmentRow>> ListAppointmentsAsync(Guid? clinicId, Guid? leadId, string? status, bool upcomingOnly,
        int page, CancellationToken ct) =>
        GetRequiredAsync<PagedResult<AppointmentRow>>("/appointments" + Query(("clinicId", clinicId), ("leadId", leadId),
            ("status", status), ("upcomingOnly", upcomingOnly), ("page", page)), ct);

    public Task<PlatformAdminChange> SetAppointmentStatusAsync(Guid id, string status, CancellationToken ct) =>
        WriteForAsync<PlatformAdminChange>(HttpMethod.Put, $"/appointments/{id}/status", new StatusBody(status), null, false, ct);
}
