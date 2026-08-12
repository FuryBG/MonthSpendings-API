using Application.Dto;
using Application.UseCases.NotificationTransactions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

        public NotificationTransactionController(
            ICreateNotificationTransactionUseCase createUseCase,
            IGetUncategorizedNotificationTransactionsUseCase getUncategorizedUseCase,
            ICategorizeNotificationTransactionUseCase categorizeUseCase)
        {
            _CreateUseCase = createUseCase;
            _GetUncategorizedUseCase = getUncategorizedUseCase;
            _CategorizeUseCase = categorizeUseCase;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateNotificationTransactionDto dto, CancellationToken cancellationToken)
        {
            var result = await _CreateUseCase.InvokeAsync(dto, cancellationToken);
            if (!result.Successful)
                return BadRequest(result.ErrorMessage);
            return Ok(result.Data);
        }

        [HttpGet]
        public async Task<IActionResult> GetUncategorized(CancellationToken cancellationToken)
        {
            var result = await _GetUncategorizedUseCase.InvokeAsync(cancellationToken);
            if (!result.Successful)
                return BadRequest(result.ErrorMessage);
            return Ok(result.Data);
        }

        [HttpPost("categorize")]
        public async Task<IActionResult> Categorize([FromBody] CategorizeNotificationTransactionDto dto, CancellationToken cancellationToken)
        {
            var result = await _CategorizeUseCase.InvokeAsync(dto, cancellationToken);
            if (!result.Successful)
                return BadRequest(result.ErrorMessage);
            return Ok(result.Data);
        }
    }
}
