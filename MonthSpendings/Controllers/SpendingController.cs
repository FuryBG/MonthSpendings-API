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

        //[Authorize]
        //[HttpGet]
        //public async Task<IActionResult> GetBudgets()
        //{
        //    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        //    var result = await _GetAllBudgetsUseCase.InvokeAsync(userId);

        //    if (result.Successful)
        //    {
        //        return Ok(result.Data);
        //    }
        //    else
        //    {
        //        return BadRequest(result.ErrorMessage);
        //    }
        //}

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SpendingDto spendingDto)
        {
            var result = await _CreateSpendingUseCase.InvokeAsync(spendingDto);

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
        [HttpDelete]
        public async Task<IActionResult> Delete([FromQuery] int spendingId)
        {
            var result = await _DeleteSpendingUseCase.InvokeAsync(spendingId);

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
