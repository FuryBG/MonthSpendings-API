using Application.UseCases.Statistics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MonthSpendings.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StatisticsController : ControllerBase
    {
        private IGetPeriodComparisonUseCase _GetPeriodComparisonUseCase;
        private readonly ILogger<StatisticsController> _Logger;
        public StatisticsController(IGetPeriodComparisonUseCase getPeriodComparisonUseCase, ILogger<StatisticsController> logger)
        {
            _GetPeriodComparisonUseCase = getPeriodComparisonUseCase;
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
    }
}
