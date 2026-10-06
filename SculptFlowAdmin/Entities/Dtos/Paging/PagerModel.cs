using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SculptFlowAdmin.Entities.Responses.Paging;

namespace SculptFlowAdmin.Entities.Dtos.Paging;

public record PagerModel(int Page, int Pages, int Total, bool HasPrevious, bool HasNext)
{
    public static PagerModel From<T>(PagedResult<T> r) => new(r.Page, r.Pages, r.Total, r.HasPrevious, r.HasNext);
}
