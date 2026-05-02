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
        private readonly ILogger<SpendingController> _Logger;
        public SpendingController(ICreateSpendingUseCase createSpendingUseCase, IDeleteSpendingUseCase deleteSpendingUseCase, ILogger<SpendingController> logger)
        {
            _CreateSpendingUseCase = createSpendingUseCase;
            _DeleteSpendingUseCase = deleteSpendingUseCase;
            _Logger = logger;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SpendingDto spendingDto)
        {
            var result = await _CreateSpendingUseCase.InvokeAsync(spendingDto);
            if (!result.Successful)
            {
                _Logger.LogWarning("CreateSpending failed: {Error}", result.ErrorMessage);
                return BadRequest(result.ErrorMessage);
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
                _Logger.LogWarning("DeleteSpending failed for spending {SpendingId}: {Error}", spendingId, result.ErrorMessage);
                return BadRequest(result.ErrorMessage);
            }
            return Ok(result.Data);
        }
    }
}
