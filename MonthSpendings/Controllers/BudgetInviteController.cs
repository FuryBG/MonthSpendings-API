using Application.Dto;
using Application.Dto.Budget;
using Application.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MonthSpendings.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BudgetInviteController : ControllerBase
    {
        private ICreateBudgetInviteUseCase _CreateBudgetInviteUseCase;
        public BudgetInviteController(ICreateBudgetInviteUseCase createBudgetInviteUseCase)
        {
            _CreateBudgetInviteUseCase = createBudgetInviteUseCase;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(BudgetInviteDto budgetInviteDto)
        {
            var result = await _CreateBudgetInviteUseCase.InvokeAsync(budgetInviteDto);

            if (result.Successful)
            {
                return Ok(result.Data);
            }
            else
            {
                return BadRequest(result.ErrorMessage);
            }
        }

        //[Authorize]
        //[HttpGet]
        //public async Task<IActionResult> GetAll()
        //{
        //    var result = await _CreateBudgetCategoryUseCase.InvokeAsync(budgetCategoryDto);

        //    if (result.Successful)
        //    {
        //        return Ok(result.Data);
        //    }
        //    else
        //    {
        //        return BadRequest(result.ErrorMessage);
        //    }
        //}

        //[Authorize]
        //[HttpDelete]
        //public async Task<IActionResult> Delete([FromQuery] int budgetCategoryId)
        //{
        //    var result = await _DeleteBudgetCategoryUseCase.InvokeAsync(budgetCategoryId);

        //    if (result.Successful)
        //    {
        //        return Ok(result.Data);
        //    }
        //    else
        //    {
        //        return BadRequest(result.ErrorMessage);
        //    }
        //}
    }
}
