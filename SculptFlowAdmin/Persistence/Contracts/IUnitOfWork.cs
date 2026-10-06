namespace SculptFlowAdmin.Persistence.Contracts;

/// <summary>Saves pending changes to the main app's tables (ApplicationDbContext) made through any repository in this
/// request. Services decide when to save.</summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
