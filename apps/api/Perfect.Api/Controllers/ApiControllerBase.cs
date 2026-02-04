using Microsoft.AspNetCore.Mvc;
using Perfect.Application.Models;

namespace Perfect.Api.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult Envelope<T>(T data)
        => Ok(new ApiResponse<T>(data));

    protected IActionResult EnvelopeCollection<T>(IReadOnlyCollection<T> data)
        => Ok(new ApiResponse<IReadOnlyCollection<T>>(data));

    protected IActionResult EnvelopeCollection<T>(IEnumerable<T> data)
        => Ok(new ApiResponse<IReadOnlyCollection<T>>(data.ToList()));

    protected IActionResult EnvelopePaged<T>(PaginatedResult<T> page)
        => Ok(new ApiResponse<IReadOnlyCollection<T>>(
            page.Items,
            new ApiMeta(page.Page, page.PageSize, page.TotalItems, page.TotalPages)));
}
