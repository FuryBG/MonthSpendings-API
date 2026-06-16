using Application.Dto.Bank;
using Application.UseCases.Bank;
using EnableBanking.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MonthSpendings.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionsController : ControllerBase
    {
        private ICategorizeTransactionsUseCase _CategorizeTransactionsUseCase;
        private IGetUncategorizedTransactionsUseCase _GetUncategorizedTransactionsUseCase;
        private ISyncTransactionsUseCase _SyncTransactionsUseCase;
        private readonly ILogger<TransactionsController> _Logger;
        public TransactionsController(IGetUncategorizedTransactionsUseCase getUncategorizedTransactionsUseCase, ICategorizeTransactionsUseCase categorizeTransactionsUseCase, ISyncTransactionsUseCase syncTransactionsUseCase, ILogger<TransactionsController> logger)
        {
            _GetUncategorizedTransactionsUseCase = getUncategorizedTransactionsUseCase;
            _CategorizeTransactionsUseCase = categorizeTransactionsUseCase;
            _SyncTransactionsUseCase = syncTransactionsUseCase;
            _Logger = logger;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetBanks(CancellationToken cancellationToken)
        {
            var result = await _GetUncategorizedTransactionsUseCase.InvokeAsync(cancellationToken);
            if (result.Successful) return Ok(result.Data);
            _Logger.LogWarning("GetUncategorizedTransactions failed: {Error}", result.ErrorMessage);
            return BadRequest(result.ErrorMessage);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UpdateTransaction([FromBody] BankTransactionDto transactionDto, CancellationToken cancellationToken)
        {
            var result = await _CategorizeTransactionsUseCase.InvokeAsync(transactionDto, cancellationToken);
            if (result.Successful) return Ok(result.Data);
            _Logger.LogWarning("CategorizeTransaction failed: {Error}", result.ErrorMessage);
            return BadRequest(result.ErrorMessage);
        }

        [Authorize]
        [HttpPost("sync")]
        public async Task<IActionResult> SyncTransactions(CancellationToken cancellationToken)
        {
            var result = await _SyncTransactionsUseCase.InvokeAsync(cancellationToken);
            if (result.Successful) return Ok();
            _Logger.LogWarning("SyncTransactions failed: {Error}", result.ErrorMessage);
            return BadRequest(result.ErrorMessage);
        }
    }
}