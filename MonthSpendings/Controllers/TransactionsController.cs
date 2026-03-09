using Application.Dto.Bank;
using Application.UseCases.Bank;
using EnableBanking.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MonthSpendings.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionsController : ControllerBase
    {
        private ICategorizeTransactionsUseCase _CategorizeTransactionsUseCase;
        private IGetUncategorizedTransactionsUseCase _GetUncategorizedTransactionsUseCase;
        public TransactionsController(IGetUncategorizedTransactionsUseCase getUncategorizedTransactionsUseCase, ICategorizeTransactionsUseCase categorizeTransactionsUseCase)
        {
            _GetUncategorizedTransactionsUseCase = getUncategorizedTransactionsUseCase;
            _CategorizeTransactionsUseCase = categorizeTransactionsUseCase;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetBanks(CancellationToken cancellationToken)
        {
            var result = await _GetUncategorizedTransactionsUseCase.InvokeAsync(cancellationToken);

            if (result.Successful)
            {
                return Ok(result.Data);
            }
            else
            {
                return BadRequest(result.ErrorMessage);
            }
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UpdateTransaction([FromBody] BankTransactionDto transactionDto, CancellationToken cancellationToken)
        {
            var result = await _CategorizeTransactionsUseCase.InvokeAsync(transactionDto, cancellationToken);

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