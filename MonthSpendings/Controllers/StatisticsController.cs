using Application.UseCases.Statistics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MonthSpendings.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StatisticsController : ControllerBase
    {
        private readonly IGetRangeSummaryUseCase _GetRangeSummaryUseCase;

        public StatisticsController(
            IGetRangeSummaryUseCase getRangeSummaryUseCase)
        {
            _GetRangeSummaryUseCase = getRangeSummaryUseCase;
        }

        [Authorize]
        [HttpGet("range-summary")]
        public async Task<IActionResult> GetRangeSummary(
            [FromQuery] int budgetId,
            [FromQuery] int fromPeriodId,
            [FromQuery] int toPeriodId,
            [FromQuery] int? categoryId,
            [FromQuery] int top = GetRangeSummaryUseCase.DefaultTop)
        {
            var result = await _GetRangeSummaryUseCase.InvokeAsync(budgetId, fromPeriodId, toPeriodId, categoryId, top);
            if (!result.Successful)
            {
                return StatusCode((int)result.ErrorType, result.ErrorMessage);
            }
            return Ok(result.Data);
        }
    }
}
