using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SculptFlowAdmin.Entities.Responses.Paging;

namespace SculptFlowAdmin.Entities.Dtos.Paging;

/// <summary>What the _Pager partial shows. PageSize (when known) lets it say "51–100 of 1,284".</summary>
public record PagerModel(int Page, int Pages, int Total, bool HasPrevious, bool HasNext, int PageSize = 0)
{
    public static PagerModel From<T>(PagedResult<T> r) => new(r.Page, r.Pages, r.Total, r.HasPrevious, r.HasNext, r.PageSize);

    public int First => PageSize <= 0 || Total == 0 ? 0 : (Page - 1) * PageSize + 1;
    public int Last => PageSize <= 0 ? 0 : Math.Min(Page * PageSize, Total);
}
