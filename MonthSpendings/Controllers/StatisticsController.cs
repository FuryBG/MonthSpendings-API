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
        public StatisticsController(IGetPeriodComparisonUseCase getPeriodComparisonUseCase)
        {
            _GetPeriodComparisonUseCase = getPeriodComparisonUseCase;
        }

        [Authorize]
        [HttpGet("period-comparison")]
        public async Task<IActionResult> GetPeriodComparison([FromQuery] int budgetId)
        {
            var result = await _GetPeriodComparisonUseCase.InvokeAsync(budgetId);
            return result.Successful ? Ok(result.Data) : BadRequest(result.ErrorMessage);
        }
    }
}
