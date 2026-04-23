using Application.Dto.Budget;
using Application.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MonthSpendings.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BudgetCategoryController : ControllerBase
    {
        private ICreateBudgetCategoryUseCase _CreateBudgetCategoryUseCase;
        private IDeleteBudgetCategoryUseCase _DeleteBudgetCategoryUseCase;
        private IUpdateBudgetCategoryNameUseCase _UpdateBudgetCategoryNameUseCase;
        public BudgetCategoryController(ICreateBudgetCategoryUseCase createBudgetCategoryUseCase, IDeleteBudgetCategoryUseCase deleteBudgetCategoryUseCase, IUpdateBudgetCategoryNameUseCase updateBudgetCategoryNameUseCase)
        {
            _CreateBudgetCategoryUseCase = createBudgetCategoryUseCase;
            _DeleteBudgetCategoryUseCase = deleteBudgetCategoryUseCase;
            _UpdateBudgetCategoryNameUseCase = updateBudgetCategoryNameUseCase;
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

        [Authorize]
        [HttpPatch("{id}/name")]
        public async Task<IActionResult> UpdateCategoryName(int id, [FromBody] string newName)
        {
            var result = await _UpdateBudgetCategoryNameUseCase.InvokeAsync(id, newName);

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
