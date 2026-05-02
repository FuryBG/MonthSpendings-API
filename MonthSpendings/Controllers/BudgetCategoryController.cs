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
        private IUpdateBudgetCategoryNameUseCase _UpdateBudgetCategoryNameUseCase;
        private readonly ILogger<BudgetCategoryController> _Logger;
        public BudgetCategoryController(ICreateBudgetCategoryUseCase createBudgetCategoryUseCase, IDeleteBudgetCategoryUseCase deleteBudgetCategoryUseCase, IUpdateBudgetCategoryNameUseCase updateBudgetCategoryNameUseCase, ILogger<BudgetCategoryController> logger)
        {
            _CreateBudgetCategoryUseCase = createBudgetCategoryUseCase;
            _DeleteBudgetCategoryUseCase = deleteBudgetCategoryUseCase;
            _UpdateBudgetCategoryNameUseCase = updateBudgetCategoryNameUseCase;
            _Logger = logger;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(BudgetCategoryDto budgetCategoryDto)
        {
            var result = await _CreateBudgetCategoryUseCase.InvokeAsync(budgetCategoryDto);
            if (!result.Successful)
            {
                _Logger.LogWarning("CreateBudgetCategory failed: {Error}", result.ErrorMessage);
                return BadRequest(result.ErrorMessage);
            }
            return Ok(result.Data);
        }

        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> Delete([FromQuery] int budgetCategoryId)
        {
            var result = await _DeleteBudgetCategoryUseCase.InvokeAsync(budgetCategoryId);
            if (!result.Successful)
            {
                _Logger.LogWarning("DeleteBudgetCategory failed for category {CategoryId}: {Error}", budgetCategoryId, result.ErrorMessage);
                return BadRequest(result.ErrorMessage);
            }
            return Ok(result.Data);
        }

        [Authorize]
        [HttpPatch("{id}/name")]
        public async Task<IActionResult> UpdateCategoryName(int id, [FromBody] string newName)
        {
            var result = await _UpdateBudgetCategoryNameUseCase.InvokeAsync(id, newName);
            if (!result.Successful)
            {
                _Logger.LogWarning("UpdateBudgetCategoryName failed for category {CategoryId}: {Error}", id, result.ErrorMessage);
                return BadRequest(result.ErrorMessage);
            }
            return Ok(result.Data);
        }
    }
}
