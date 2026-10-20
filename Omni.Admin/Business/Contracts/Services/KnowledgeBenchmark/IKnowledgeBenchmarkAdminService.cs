using System.Text.Json.Nodes;
using Omni.Admin.Entities.Requests.KnowledgeBenchmark;

namespace Omni.Admin.Business.Contracts.Services.KnowledgeBenchmark;

/// <summary>
/// One client's Knowledge Retrieval Benchmark (the Search benchmark page), through the main app's platform-admin API.
/// Reads return the main app's JSON for the page's script; every change is written to admin_audit_log under that client.
/// Reads answer null for an unknown client or record; writes throw KeyNotFoundException for one.
/// </summary>
public interface IKnowledgeBenchmarkAdminService
{
    Task<JsonNode?> DashboardAsync(Guid clientId, CancellationToken ct = default);

    Task<JsonNode?> ListCasesAsync(Guid clientId, string? view, Guid? generationId, int skip, int take, CancellationToken ct = default);

    Task<JsonNode?> GetCaseAsync(Guid clientId, Guid id, CancellationToken ct = default);

    Task<JsonNode> CreateManualCaseAsync(Guid clientId, CreateManualBenchmarkCaseRequest request, CancellationToken ct = default);

    Task<JsonNode> UpdateCaseAsync(Guid clientId, Guid id, UpdateBenchmarkCaseRequest request, CancellationToken ct = default);

    Task<JsonNode> SetReviewedAsync(Guid clientId, Guid id, SetBenchmarkCaseReviewedRequest request, CancellationToken ct = default);

    Task DeleteCaseAsync(Guid clientId, Guid id, CancellationToken ct = default);

    Task<JsonNode?> SourceDocumentsAsync(Guid clientId, CancellationToken ct = default);

    Task<JsonNode?> SourceChunksAsync(Guid clientId, Guid documentId, CancellationToken ct = default);

    Task<JsonNode> GenerateCasesAsync(Guid clientId, CancellationToken ct = default);

    Task<JsonNode?> ListGenerationsAsync(Guid clientId, int take, CancellationToken ct = default);

    Task<JsonNode?> GetGenerationAsync(Guid clientId, Guid id, CancellationToken ct = default);

    Task<JsonNode> CancelGenerationAsync(Guid clientId, Guid id, CancellationToken ct = default);

    Task<JsonNode> StartRunAsync(Guid clientId, StartBenchmarkRunRequest request, CancellationToken ct = default);

    Task<JsonNode?> ListRunsAsync(Guid clientId, int take, CancellationToken ct = default);

    Task<JsonNode?> GetRunAsync(Guid clientId, Guid id, CancellationToken ct = default);

    Task<JsonNode?> ListResultsAsync(Guid clientId, Guid runId, string? classification, int skip, int take, CancellationToken ct = default);

    Task<JsonNode?> RunGenerationsAsync(Guid clientId, Guid runId, CancellationToken ct = default);

    Task<JsonNode?> GetResultAsync(Guid clientId, Guid runId, Guid resultId, CancellationToken ct = default);
}
