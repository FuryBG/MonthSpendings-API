using Application;
using Microsoft.AspNetCore.Mvc;

namespace MonthSpendings.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        public UserController()
        {
            
        }

        [HttpGet]
        public IActionResult Index()
        {
            return Ok();
        }

        // POST: UserController/Create
        [HttpPost]
        public ActionResult Create([FromBody] GoogleUserDto googleUserDto)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return Ok();
            }
        }
    }
}
