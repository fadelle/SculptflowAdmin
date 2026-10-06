namespace SculptFlowAdmin.Entities.Responses.Billing;

public record PagedResponse<T>(IReadOnlyList<T> Items, int Total, int Page, int PageSize)
{
    public int Pages => Math.Max(1, (int)Math.Ceiling(Total / (double)Math.Max(1, PageSize)));
}
