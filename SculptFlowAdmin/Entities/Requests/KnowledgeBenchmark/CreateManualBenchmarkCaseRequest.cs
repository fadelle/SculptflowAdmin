namespace SculptFlowAdmin.Entities.Requests.KnowledgeBenchmark;

/// <summary>Copy of the main app's body for POST knowledge-benchmark/clinics/{clinicId}/cases (see MAIN_APP_SYNC.md).</summary>
public record CreateManualBenchmarkCaseRequest(string? Question, Guid DocumentId, Guid ChunkId);
