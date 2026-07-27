using Application.Dto;
using Application.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MonthSpendings.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private IRegisterUserUseCase _RegisterUseCase { get; set; }
        private IGetUserByIdUseCase _GetUserByIdUseCase { get; set; }
        private IUpdateLastUserActivityUseCase _UpdateLastUserActivityUseCase { get; set; }
        private IRequestAccountDeletionUseCase _RequestAccountDeletionUseCase { get; set; }
        private IUpdateNotificationTokenUseCase _UpdateNotificationTokenUseCase { get; set; }
        private readonly ILogger<UserController> _Logger;
        public UserController(IRegisterUserUseCase registerUseCase, IGetUserByIdUseCase getUserByIdUseCase, IUpdateLastUserActivityUseCase updateLastUserActivityUseCase, IRequestAccountDeletionUseCase requestAccountDeletionUseCase, IUpdateNotificationTokenUseCase updateNotificationTokenUseCase, ILogger<UserController> logger)
        {
            _RegisterUseCase = registerUseCase;
            _GetUserByIdUseCase = getUserByIdUseCase;
            _UpdateLastUserActivityUseCase = updateLastUserActivityUseCase;
            _RequestAccountDeletionUseCase = requestAccountDeletionUseCase;
            _UpdateNotificationTokenUseCase = updateNotificationTokenUseCase;
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

        [HttpPut("activity")]
        [Authorize]
        public async Task<IActionResult> UpdateActivity([FromBody] UpdateUserActivityDto dto)
        {
            var result = await _UpdateLastUserActivityUseCase.InvokeAsync(dto);
            if (!result.Successful)
            {
                _Logger.LogWarning("UpdateActivity failed: {Error}", result.ErrorMessage);
                return BadRequest(result.ErrorMessage);
            }
            return Ok();
        }

        [HttpPut("notification-token")]
        [Authorize]
        public async Task<IActionResult> UpdateNotificationToken([FromBody] UpdateNotificationTokenDto dto)
        {
            var result = await _UpdateNotificationTokenUseCase.InvokeAsync(dto);
            if (!result.Successful)
            {
                _Logger.LogWarning("UpdateNotificationToken failed: {Error}", result.ErrorMessage);
                return BadRequest(result.ErrorMessage);
            }
            _Logger.LogInformation("User push notification token updated successfully: {PushNotificationToken}", result.Data);
            return Ok();
        }

        [HttpPost("delete-request")]
        [Authorize]
        public async Task<IActionResult> RequestDeletion()
        {
            var result = await _RequestAccountDeletionUseCase.InvokeAsync();
            if (!result.Successful)
            {
                _Logger.LogWarning("RequestDeletion failed: {Error}", result.ErrorMessage);
                return BadRequest(result.ErrorMessage);
            }
            return Ok();
        }
    }
}
