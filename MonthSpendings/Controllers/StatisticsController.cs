using Application.UseCases.Statistics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MonthSpendings.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StatisticsController : ControllerBase
    {
        private readonly IGetPeriodComparisonUseCase _GetPeriodComparisonUseCase;
        private readonly IGetPeriodsHistoryUseCase _GetPeriodsHistoryUseCase;
        private readonly ILogger<StatisticsController> _Logger;

        public StatisticsController(
            IGetPeriodComparisonUseCase getPeriodComparisonUseCase,
            IGetPeriodsHistoryUseCase getPeriodsHistoryUseCase,
            ILogger<StatisticsController> logger)
        {
            _GetPeriodComparisonUseCase = getPeriodComparisonUseCase;
            _GetPeriodsHistoryUseCase = getPeriodsHistoryUseCase;
            _Logger = logger;
        }

        [Authorize]
        [HttpGet("period-comparison")]
        public async Task<IActionResult> GetPeriodComparison([FromQuery] int budgetId)
        {
            var result = await _GetPeriodComparisonUseCase.InvokeAsync(budgetId);
            if (!result.Successful)
            {
                _Logger.LogWarning("GetPeriodComparison failed: {Error}", result.ErrorMessage);
                return BadRequest(result.ErrorMessage);
            }
            return Ok(result.Data);
        }

        [Authorize]
        [HttpGet("periods-history")]
        public async Task<IActionResult> GetPeriodsHistory([FromQuery] int budgetId)
        {
            var result = await _GetPeriodsHistoryUseCase.InvokeAsync(budgetId);
            if (!result.Successful)
            {
                _Logger.LogWarning("GetPeriodsHistory failed: {Error}", result.ErrorMessage);
                return BadRequest(result.ErrorMessage);
            }
            return Ok(result.Data);
        }
    }
}
