using Microsoft.EntityFrameworkCore;
using SculptFlowAdmin.Common.Helpers;
using SculptFlowAdmin.Entities.Responses.Paging;

namespace SculptFlowAdmin.Persistence.Helpers;

public static class QueryPaging
{
    public static async Task<PagedResult<T>> ToPagedAsync<T>(this IQueryable<T> query, int page, int pageSize = Paging.DefaultPageSize,
        CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 500);
        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return new PagedResult<T>(items, page, pageSize, total);
    }
}
