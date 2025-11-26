using Application.Dto;
using Application.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace MonthSpendings.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private IRegisterUserUseCase _RegisterUseCase { get; set; }
        private IGetUserByIdUseCase _GetUserByIdUseCase { get; set; }
        public UserController(IRegisterUserUseCase registerUseCase, IGetUserByIdUseCase getUserByIdUseCase)
        {
            _RegisterUseCase = registerUseCase;
            _GetUserByIdUseCase = getUserByIdUseCase;
        }

        [HttpGet]
        public async Task<IActionResult> GetAuthenticatedUser()
        {
            var result = await _GetUserByIdUseCase.InvokeAsync();

            if (result.Successful)
            {
                return Ok(result.Data);
            }
            else
            {
                return BadRequest(result.ErrorMessage);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] GoogleUserDto googleUserDto)
        {
            var result = await _RegisterUseCase.InvokeAsync(googleUserDto);

            if (result.Successful)
            {
                return Ok(result.Data);
            }
            else
            {
                return BadRequest(result.ErrorMessage);
            }
        }
    }
}
