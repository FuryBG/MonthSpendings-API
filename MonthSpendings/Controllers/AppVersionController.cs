using Application.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace MonthSpendings.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppVersionController : ControllerBase
    {
        private readonly AppVersionOptions _Options;

        public AppVersionController(IOptionsSnapshot<AppVersionOptions> options)
        {
            _Options = options.Value;
        }

        [HttpGet]
        public IActionResult GetAppVersion()
        {
            return Ok(_Options);
        }
    }
}
