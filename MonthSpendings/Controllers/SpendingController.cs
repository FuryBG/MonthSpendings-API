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
        public SpendingController(ICreateSpendingUseCase createSpendingUseCase, IDeleteSpendingUseCase deleteSpendingUseCase)
        {
            _CreateSpendingUseCase = createSpendingUseCase;
            _DeleteSpendingUseCase = deleteSpendingUseCase;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SpendingDto spendingDto)
        {
            var result = await _CreateSpendingUseCase.InvokeAsync(spendingDto);
            return result.Successful ? Ok(result.Data) : BadRequest(result.ErrorMessage);
        }

        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> Delete([FromQuery] int spendingId)
        {
            var result = await _DeleteSpendingUseCase.InvokeAsync(spendingId);
            return result.Successful ? Ok(result.Data) : BadRequest(result.ErrorMessage);
        }
    }
}
