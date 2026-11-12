namespace Omni.Admin.Entities.Responses.Billing;

/// <summary>(Copy of the main app's Entities/Dtos/Billing/LedgerResult.) The answer to a top-up or adjustment: the ledger
/// entry, the balance after it, and whether the request was a replay of one already applied (same Idempotency-Key).</summary>
public sealed record LedgerResult(Guid LedgerEntryId, decimal BalanceAfter, bool Duplicate);
