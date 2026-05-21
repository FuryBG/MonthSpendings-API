using Application.Dto.Bank;
using Application.UseCases.TransactionRules;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MonthSpendings.Controllers
{
    [ApiController]
    [Route("api/transaction-rules")]
    [Authorize]
    public class TransactionRuleController : ControllerBase
    {
        private readonly IGetTransactionCategoryRulesUseCase _GetRules;
        private readonly ICreateTransactionCategoryRuleUseCase _CreateRule;
        private readonly IDeleteTransactionCategoryRuleUseCase _DeleteRule;

        public TransactionRuleController(
            IGetTransactionCategoryRulesUseCase getRules,
            ICreateTransactionCategoryRuleUseCase createRule,
            IDeleteTransactionCategoryRuleUseCase deleteRule)
        {
            _GetRules = getRules;
            _CreateRule = createRule;
            _DeleteRule = deleteRule;
        }

        [HttpGet]
        public async Task<IActionResult> GetRules(CancellationToken cancellationToken)
        {
            var result = await _GetRules.InvokeAsync(cancellationToken);
            return result.Successful ? Ok(result.Data) : BadRequest(result.ErrorMessage);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRule([FromBody] CreateTransactionCategoryRuleDto dto, CancellationToken cancellationToken)
        {
            var result = await _CreateRule.InvokeAsync(dto, cancellationToken);
            return result.Successful ? Ok(result.Data) : BadRequest(result.ErrorMessage);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRule(int id, CancellationToken cancellationToken)
        {
            var result = await _DeleteRule.InvokeAsync(id, cancellationToken);
            return result.Successful ? Ok(result.Data) : BadRequest(result.ErrorMessage);
        }
    }
}
