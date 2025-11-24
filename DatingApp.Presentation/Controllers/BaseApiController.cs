using DatingApp.Presentation.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.Presentation.Controllers
{
    [ServiceFilter(typeof(LogUserActivity))]
    [Route("api/[controller]")]
    [ApiController]
    public class BaseApiController : ControllerBase
    {
    }
}
