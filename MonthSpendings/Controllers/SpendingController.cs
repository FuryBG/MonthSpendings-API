using Application.Dto.Budget;
using Application.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MonthSpendings.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SpendingController : ControllerBase
    {
        private ICreateSpendingUseCase _CreateSpendingUseCase;
        private IDeleteSpendingUseCase _DeleteSpendingUseCase;
        private IGetCategorySpendingsByPeriodUseCase _GetCategorySpendingsByPeriodUseCase;
        public SpendingController(ICreateSpendingUseCase createSpendingUseCase, IDeleteSpendingUseCase deleteSpendingUseCase, IGetCategorySpendingsByPeriodUseCase getCategorySpendingsByPeriodUseCase)
        {
            _CreateSpendingUseCase = createSpendingUseCase;
            _DeleteSpendingUseCase = deleteSpendingUseCase;
            _GetCategorySpendingsByPeriodUseCase = getCategorySpendingsByPeriodUseCase;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SpendingDto spendingDto)
        {
            var result = await _CreateSpendingUseCase.InvokeAsync(spendingDto);
            if (!result.Successful)
            {
                return StatusCode((int)result.ErrorType, result.ErrorMessage);
            }
            return Ok(result.Data);
        }

        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> Delete([FromQuery] int spendingId)
        {
            var result = await _DeleteSpendingUseCase.InvokeAsync(spendingId);
            if (!result.Successful)
            {
                return StatusCode((int)result.ErrorType, result.ErrorMessage);
            }
            return Ok(result.Data);
        }

        [Authorize]
        [HttpGet("by-period")]
        public async Task<IActionResult> GetByPeriod([FromQuery] int budgetCategoryId, [FromQuery] int budgetPeriodId)
        {
            var result = await _GetCategorySpendingsByPeriodUseCase.InvokeAsync(budgetCategoryId, budgetPeriodId);
            if (!result.Successful)
            {
                return StatusCode((int)result.ErrorType, result.ErrorMessage);
            }
            return Ok(result.Data);
        }
    }
}
