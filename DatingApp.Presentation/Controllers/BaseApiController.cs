using DatingApp.Application.Helpers;
using DatingApp.Presentation.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.Presentation.Controllers
{
    [ServiceFilter(typeof(LogUserActivity))]
    [Route("api/[controller]")]
    [ApiController]
    public class BaseApiController : ControllerBase
    {
        protected ActionResult HandleResult<T>(Result<T> result)
        {
            if (result == null) return NotFound();
            if (result.IsSuccess && result.Value != null)
                return Ok(result.Value);
            if (result.IsSuccess && result.Value == null)
                return NotFound();

            return BadRequest(result.Error);
        }

        protected ActionResult HandlePaginatedResult<T>(PaginatedResult<T>? result)
        {
            if (result?.Items != null) return Ok(result);
            return NotFound();
        }
    }
}
