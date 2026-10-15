using Application.Dto;
using Application.UseCases.NotificationTransactions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonthSpendings.Authentication;

namespace MonthSpendings.Controllers
{
    [ApiController]
    [Route("api/notification-transactions")]
    [Authorize]
    public class NotificationTransactionController : ControllerBase
    {
        private readonly ICreateNotificationTransactionUseCase _CreateUseCase;
        private readonly IGetUncategorizedNotificationTransactionsUseCase _GetUncategorizedUseCase;
        private readonly ICategorizeNotificationTransactionUseCase _CategorizeUseCase;
        private readonly IDeleteNotificationTransactionUseCase _DeleteUseCase;
        private readonly IGetWalletIngestTokenUseCase _GetWalletIngestTokenUseCase;

        public NotificationTransactionController(
            ICreateNotificationTransactionUseCase createUseCase,
            IGetUncategorizedNotificationTransactionsUseCase getUncategorizedUseCase,
            ICategorizeNotificationTransactionUseCase categorizeUseCase,
            IDeleteNotificationTransactionUseCase deleteUseCase,
            IGetWalletIngestTokenUseCase getWalletIngestTokenUseCase)
        {
            _CreateUseCase = createUseCase;
            _GetUncategorizedUseCase = getUncategorizedUseCase;
            _CategorizeUseCase = categorizeUseCase;
            _DeleteUseCase = deleteUseCase;
            _GetWalletIngestTokenUseCase = getWalletIngestTokenUseCase;
        }

        // Posted by the phone's background wallet listener with the wallet ingest token (or a normal access token).
        [HttpPost]
        [Authorize(AuthenticationSchemes = $"{JwtBearerDefaults.AuthenticationScheme},{WalletIngestAuth.Scheme}")]
        public async Task<IActionResult> Create([FromBody] CreateNotificationTransactionDto dto, CancellationToken cancellationToken)
        {
            var result = await _CreateUseCase.InvokeAsync(dto, cancellationToken);
            if (!result.Successful)
            {
                return StatusCode((int)result.ErrorType, result.ErrorMessage);
            }
            return Ok(result.Data);
        }

        [HttpGet("ingest-token")]
        public async Task<IActionResult> GetIngestToken()
        {
            var result = await _GetWalletIngestTokenUseCase.InvokeAsync();
            if (!result.Successful)
            {
                return StatusCode((int)result.ErrorType, result.ErrorMessage);
            }
            return Ok(result.Data);
        }

        [HttpGet]
        public async Task<IActionResult> GetUncategorized(CancellationToken cancellationToken)
        {
            var result = await _GetUncategorizedUseCase.InvokeAsync(cancellationToken);
            if (!result.Successful)
            {
                return StatusCode((int)result.ErrorType, result.ErrorMessage);
            }
            return Ok(result.Data);
        }

        [HttpPost("categorize")]
        public async Task<IActionResult> Categorize([FromBody] CategorizeNotificationTransactionDto dto, CancellationToken cancellationToken)
        {
            var result = await _CategorizeUseCase.InvokeAsync(dto, cancellationToken);
            if (!result.Successful)
            {
                return StatusCode((int)result.ErrorType, result.ErrorMessage);
            }
            return Ok(result.Data);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            var result = await _DeleteUseCase.InvokeAsync(id, cancellationToken);
            if (!result.Successful)
            {
                return StatusCode((int)result.ErrorType, result.ErrorMessage);
            }
            return Ok();
        }
    }
}
