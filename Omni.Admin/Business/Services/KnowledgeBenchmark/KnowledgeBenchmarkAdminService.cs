using System.Globalization;
using System.Text.Json.Nodes;
using Omni.Admin.Business.Contracts.HttpClients.MainApp;
using Omni.Admin.Business.Contracts.Managers;
using Omni.Admin.Business.Contracts.Services.KnowledgeBenchmark;
using Omni.Admin.Entities.Requests.KnowledgeBenchmark;

namespace Omni.Admin.Business.Services.KnowledgeBenchmark;

public class KnowledgeBenchmarkAdminService : IKnowledgeBenchmarkAdminService
{
    private readonly IKnowledgeBenchmarkApiClient _api;
    private readonly IAdminAudit _audit;

    public KnowledgeBenchmarkAdminService(IKnowledgeBenchmarkApiClient api, IAdminAudit audit)
    {
        _api = api;
        _audit = audit;
    }

    public Task<JsonNode?> DashboardAsync(Guid clinicId, CancellationToken ct = default) => _api.GetAsync(clinicId, "", ct);

    public Task<JsonNode?> ListCasesAsync(Guid clinicId, string? view, Guid? generationId, int skip, int take, CancellationToken ct = default) =>
        _api.GetAsync(clinicId, "/cases" + Query(("view", view), ("generationId", generationId?.ToString()),
            ("skip", Num(Math.Max(skip, 0))), ("take", Num(Math.Clamp(take, 1, 200)))), ct);

    public Task<JsonNode?> GetCaseAsync(Guid clinicId, Guid id, CancellationToken ct = default) => _api.GetAsync(clinicId, $"/cases/{id}", ct);

    public async Task<JsonNode> CreateManualCaseAsync(Guid clinicId, CreateManualBenchmarkCaseRequest request, CancellationToken ct = default)
    {
        var created = await _api.SendAsync(clinicId, HttpMethod.Post, "/cases", request, ct);
        await _audit.LogAsync("benchmark.case_created", "knowledge_benchmark_case", Id(created), clinicId,
            new { question = request.Question, request.DocumentId, request.ChunkId }, ct);
        return created;
    }

    public async Task<JsonNode> UpdateCaseAsync(Guid clinicId, Guid id, UpdateBenchmarkCaseRequest request, CancellationToken ct = default)
    {
        var updated = await _api.SendAsync(clinicId, HttpMethod.Put, $"/cases/{id}", request, ct);
        await _audit.LogAsync("benchmark.case_updated", "knowledge_benchmark_case", id, clinicId, new { question = request.Question }, ct);
        return updated;
    }

    public async Task<JsonNode> SetReviewedAsync(Guid clinicId, Guid id, SetBenchmarkCaseReviewedRequest request, CancellationToken ct = default)
    {
        var updated = await _api.SendAsync(clinicId, HttpMethod.Post, $"/cases/{id}/review", request, ct);
        await _audit.LogAsync(request.Reviewed ? "benchmark.case_reviewed" : "benchmark.case_unreviewed", "knowledge_benchmark_case", id,
            clinicId, ct: ct);
        return updated;
    }

    public async Task DeleteCaseAsync(Guid clinicId, Guid id, CancellationToken ct = default)
    {
        await _api.DeleteAsync(clinicId, $"/cases/{id}", ct);
        await _audit.LogAsync("benchmark.case_deleted", "knowledge_benchmark_case", id, clinicId, ct: ct);
    }

    public Task<JsonNode?> SourceDocumentsAsync(Guid clinicId, CancellationToken ct = default) => _api.GetAsync(clinicId, "/source-documents", ct);

    public Task<JsonNode?> SourceChunksAsync(Guid clinicId, Guid documentId, CancellationToken ct = default) =>
        _api.GetAsync(clinicId, $"/source-documents/{documentId}/chunks", ct);

    public async Task<JsonNode> GenerateCasesAsync(Guid clinicId, CancellationToken ct = default)
    {
        var started = await _api.SendAsync(clinicId, HttpMethod.Post, "/cases/generate", null, ct);
        // No generationId = nothing could be sampled, so nothing was asked of the generator and nothing changed.
        if (started["generationId"] is not null)
        {
            await _audit.LogAsync("benchmark.generation_started", "knowledge_benchmark_generation", (string?)started["generationId"], clinicId,
                new { status = (string?)started["status"], chunksSent = (int?)started["chunksSent"] }, ct);
        }
        return started;
    }

    public Task<JsonNode?> ListGenerationsAsync(Guid clinicId, int take, CancellationToken ct = default) =>
        _api.GetAsync(clinicId, "/generations" + Query(("take", Num(Math.Clamp(take, 1, 100)))), ct);

    public Task<JsonNode?> GetGenerationAsync(Guid clinicId, Guid id, CancellationToken ct = default) => _api.GetAsync(clinicId, $"/generations/{id}", ct);

    public async Task<JsonNode> CancelGenerationAsync(Guid clinicId, Guid id, CancellationToken ct = default)
    {
        var summary = await _api.SendAsync(clinicId, HttpMethod.Post, $"/generations/{id}/cancel", null, ct);
        await _audit.LogAsync("benchmark.generation_cancelled", "knowledge_benchmark_generation", id, clinicId, ct: ct);
        return summary;
    }

    public async Task<JsonNode> StartRunAsync(Guid clinicId, StartBenchmarkRunRequest request, CancellationToken ct = default)
    {
        var run = await _api.SendAsync(clinicId, HttpMethod.Post, "/runs", request, ct);
        await _audit.LogAsync("benchmark.run_started", "knowledge_benchmark_run", Id(run), clinicId,
            new { scope = request.Scope, request.GenerationId }, ct);
        return run;
    }

    public Task<JsonNode?> ListRunsAsync(Guid clinicId, int take, CancellationToken ct = default) =>
        _api.GetAsync(clinicId, "/runs" + Query(("take", Num(Math.Clamp(take, 1, 100)))), ct);

    public Task<JsonNode?> GetRunAsync(Guid clinicId, Guid id, CancellationToken ct = default) => _api.GetAsync(clinicId, $"/runs/{id}", ct);

    public Task<JsonNode?> ListResultsAsync(Guid clinicId, Guid runId, string? classification, int skip, int take, CancellationToken ct = default) =>
        _api.GetAsync(clinicId, $"/runs/{runId}/results" + Query(("classification", classification),
            ("skip", Num(Math.Max(skip, 0))), ("take", Num(Math.Clamp(take, 1, 500)))), ct);

    public Task<JsonNode?> RunGenerationsAsync(Guid clinicId, Guid runId, CancellationToken ct = default) =>
        _api.GetAsync(clinicId, $"/runs/{runId}/generations", ct);

    public Task<JsonNode?> GetResultAsync(Guid clinicId, Guid runId, Guid resultId, CancellationToken ct = default) =>
        _api.GetAsync(clinicId, $"/runs/{runId}/results/{resultId}", ct);

    private static string? Id(JsonNode node) => (string?)node["id"];

    private static string Num(int value) => value.ToString(CultureInfo.InvariantCulture);

    private static string Query(params (string Key, string? Value)[] parts)
    {
        var pairs = parts.Where(p => !string.IsNullOrWhiteSpace(p.Value))
            .Select(p => $"{p.Key}={Uri.EscapeDataString(p.Value!.Trim())}")
            .ToList();
        return pairs.Count == 0 ? "" : "?" + string.Join("&", pairs);
    }
}
