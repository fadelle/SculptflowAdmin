namespace Omni.Admin.Entities.Requests.KnowledgeBenchmark;

/// <summary>Copy of the main app's body for POST knowledge-benchmark/clinics/{clinicId}/cases/{id}/review.</summary>
public record SetBenchmarkCaseReviewedRequest(bool Reviewed);
