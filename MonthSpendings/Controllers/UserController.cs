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
        private readonly ILogger<UserController> _Logger;
        public UserController(IRegisterUserUseCase registerUseCase, IGetUserByIdUseCase getUserByIdUseCase, ILogger<UserController> logger)
        {
            _RegisterUseCase = registerUseCase;
            _GetUserByIdUseCase = getUserByIdUseCase;
            _Logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAuthenticatedUser()
        {
            var result = await _GetUserByIdUseCase.InvokeAsync();
            if (!result.Successful)
            {
                _Logger.LogWarning("GetAuthenticatedUser failed: {Error}", result.ErrorMessage);
                return BadRequest(result.ErrorMessage);
            }
            return Ok(result.Data);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] GoogleUserDto googleUserDto)
        {
            var result = await _RegisterUseCase.InvokeAsync(googleUserDto);
            if (!result.Successful)
            {
                _Logger.LogWarning("User registration failed: {Error}", result.ErrorMessage);
                return BadRequest(result.ErrorMessage);
            }
            _Logger.LogInformation("User registered successfully: {UserInfo}", result.Data);
            return Ok(result.Data);
        }
    }
}
