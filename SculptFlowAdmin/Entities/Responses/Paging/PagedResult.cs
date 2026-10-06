namespace SculptFlowAdmin.Entities.Responses.Paging;

public record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int Total)
{
    public int Pages => Math.Max(1, (int)Math.Ceiling(Total / (double)PageSize));
    public bool HasPrevious => Page > 1;
    public bool HasNext => Page < Pages;
}
