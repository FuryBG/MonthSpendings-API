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
        private readonly ILogger<StatisticsController> _Logger;

        public StatisticsController(
            IGetRangeSummaryUseCase getRangeSummaryUseCase,
            ILogger<StatisticsController> logger)
        {
            _GetRangeSummaryUseCase = getRangeSummaryUseCase;
            _Logger = logger;
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
                _Logger.LogWarning("GetRangeSummary failed: {Error}", result.ErrorMessage);
                return BadRequest(result.ErrorMessage);
            }
            return Ok(result.Data);
        }
    }
}
