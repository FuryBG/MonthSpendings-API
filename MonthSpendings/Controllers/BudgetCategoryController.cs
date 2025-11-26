using Application.Dto.Budget;
using Application.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MonthSpendings.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BudgetCategoryController : ControllerBase
    {
        private ICreateBudgetCategoryUseCase _CreateBudgetCategoryUseCase;
        private IDeleteBudgetCategoryUseCase _DeleteBudgetCategoryUseCase;
        public BudgetCategoryController(ICreateBudgetCategoryUseCase createBudgetCategoryUseCase, IDeleteBudgetCategoryUseCase deleteBudgetCategoryUseCase)
        {
            _CreateBudgetCategoryUseCase = createBudgetCategoryUseCase;
            _DeleteBudgetCategoryUseCase = deleteBudgetCategoryUseCase;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(BudgetCategoryDto budgetCategoryDto)
        {
            var result = await _CreateBudgetCategoryUseCase.InvokeAsync(budgetCategoryDto);

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
        public async Task<IActionResult> Delete([FromQuery] int budgetCategoryId)
        {
            var result = await _DeleteBudgetCategoryUseCase.InvokeAsync(budgetCategoryId);

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
