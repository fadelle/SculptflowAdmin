using System.Text.Json.Nodes;
using Omni.Admin.Entities.Requests.KnowledgeBenchmark;

namespace Omni.Admin.Business.Contracts.Services.KnowledgeBenchmark;

/// <summary>
/// One clinic's Knowledge Retrieval Benchmark (the Search benchmark page), through the main app's platform-admin API.
/// Reads return the main app's JSON for the page's script; every change is written to admin_audit_log under that clinic.
/// Reads answer null for an unknown clinic or record; writes throw KeyNotFoundException for one.
/// </summary>
public interface IKnowledgeBenchmarkAdminService
{
    Task<JsonNode?> DashboardAsync(Guid clinicId, CancellationToken ct = default);

    Task<JsonNode?> ListCasesAsync(Guid clinicId, string? view, Guid? generationId, int skip, int take, CancellationToken ct = default);

    Task<JsonNode?> GetCaseAsync(Guid clinicId, Guid id, CancellationToken ct = default);

    Task<JsonNode> CreateManualCaseAsync(Guid clinicId, CreateManualBenchmarkCaseRequest request, CancellationToken ct = default);

    Task<JsonNode> UpdateCaseAsync(Guid clinicId, Guid id, UpdateBenchmarkCaseRequest request, CancellationToken ct = default);

    Task<JsonNode> SetReviewedAsync(Guid clinicId, Guid id, SetBenchmarkCaseReviewedRequest request, CancellationToken ct = default);

    Task DeleteCaseAsync(Guid clinicId, Guid id, CancellationToken ct = default);

    Task<JsonNode?> SourceDocumentsAsync(Guid clinicId, CancellationToken ct = default);

    Task<JsonNode?> SourceChunksAsync(Guid clinicId, Guid documentId, CancellationToken ct = default);

    Task<JsonNode> GenerateCasesAsync(Guid clinicId, CancellationToken ct = default);

    Task<JsonNode?> ListGenerationsAsync(Guid clinicId, int take, CancellationToken ct = default);

    Task<JsonNode?> GetGenerationAsync(Guid clinicId, Guid id, CancellationToken ct = default);

    Task<JsonNode> CancelGenerationAsync(Guid clinicId, Guid id, CancellationToken ct = default);

    Task<JsonNode> StartRunAsync(Guid clinicId, StartBenchmarkRunRequest request, CancellationToken ct = default);

    Task<JsonNode?> ListRunsAsync(Guid clinicId, int take, CancellationToken ct = default);

    Task<JsonNode?> GetRunAsync(Guid clinicId, Guid id, CancellationToken ct = default);

    Task<JsonNode?> ListResultsAsync(Guid clinicId, Guid runId, string? classification, int skip, int take, CancellationToken ct = default);

    Task<JsonNode?> RunGenerationsAsync(Guid clinicId, Guid runId, CancellationToken ct = default);

    Task<JsonNode?> GetResultAsync(Guid clinicId, Guid runId, Guid resultId, CancellationToken ct = default);
}
