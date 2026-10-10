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
        private IUpdateSyncWalletTransactionsUseCase _UpdateSyncWalletTransactionsUseCase { get; set; }
        private IUpdateUserLanguageUseCase _UpdateUserLanguageUseCase { get; set; }
        public UserController(IRegisterUserUseCase registerUseCase, IGetUserByIdUseCase getUserByIdUseCase, IUpdateLastUserActivityUseCase updateLastUserActivityUseCase, IRequestAccountDeletionUseCase requestAccountDeletionUseCase, IUpdateNotificationTokenUseCase updateNotificationTokenUseCase, IUpdateSyncWalletTransactionsUseCase updateSyncWalletTransactionsUseCase, IUpdateUserLanguageUseCase updateUserLanguageUseCase)
        {
            _RegisterUseCase = registerUseCase;
            _GetUserByIdUseCase = getUserByIdUseCase;
            _UpdateLastUserActivityUseCase = updateLastUserActivityUseCase;
            _RequestAccountDeletionUseCase = requestAccountDeletionUseCase;
            _UpdateNotificationTokenUseCase = updateNotificationTokenUseCase;
            _UpdateSyncWalletTransactionsUseCase = updateSyncWalletTransactionsUseCase;
            _UpdateUserLanguageUseCase = updateUserLanguageUseCase;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAuthenticatedUser()
        {
            var result = await _GetUserByIdUseCase.InvokeAsync();
            if (!result.Successful)
            {
                return StatusCode((int)result.ErrorType, result.ErrorMessage);
            }
            return Ok(result.Data);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] GoogleUserDto googleUserDto)
        {
            var result = await _RegisterUseCase.InvokeAsync(googleUserDto);
            if (!result.Successful)
            {
                return StatusCode((int)result.ErrorType, result.ErrorMessage);
            }
            return Ok(result.Data);
        }

        [HttpPut("activity")]
        [Authorize]
        public async Task<IActionResult> UpdateActivity([FromBody] UpdateUserActivityDto dto)
        {
            var result = await _UpdateLastUserActivityUseCase.InvokeAsync(dto);
            if (!result.Successful)
            {
                return StatusCode((int)result.ErrorType, result.ErrorMessage);
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
                return StatusCode((int)result.ErrorType, result.ErrorMessage);
            }
            return Ok();
        }

        [HttpPut("sync-wallet-transactions")]
        [Authorize]
        public async Task<IActionResult> UpdateSyncWalletTransactions([FromBody] UpdateSyncWalletTransactionsDto dto)
        {
            var result = await _UpdateSyncWalletTransactionsUseCase.InvokeAsync(dto);
            if (!result.Successful)
            {
                return StatusCode((int)result.ErrorType, result.ErrorMessage);
            }
            return Ok();
        }

        [HttpPut("language")]
        [Authorize]
        public async Task<IActionResult> UpdateLanguage([FromBody] UpdateLanguageDto dto)
        {
            var result = await _UpdateUserLanguageUseCase.InvokeAsync(dto);
            if (!result.Successful)
            {
                return StatusCode((int)result.ErrorType, result.ErrorMessage);
            }
            return Ok(new { language = result.Data });
        }

        [HttpPost("delete-request")]
        [Authorize]
        public async Task<IActionResult> RequestDeletion()
        {
            var result = await _RequestAccountDeletionUseCase.InvokeAsync();
            if (!result.Successful)
            {
                return StatusCode((int)result.ErrorType, result.ErrorMessage);
            }
            return Ok();
        }
    }
}
