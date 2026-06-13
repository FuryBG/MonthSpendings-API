using Application.Dto.Budget;
using Application.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonthSpendings.Contracts.Requests;
using System.Text.Json.Serialization;

namespace MonthSpendings.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BudgetController : ControllerBase
    {
        private ICreateBudgetUseCase _CreateBudgetUseCase;
        private IGetAllBudgetsUseCase _GetAllBudgetsUseCase;
        private IDeleteBudgetUseCase _DeleteBudgetUseCase;
        private IFinishBudgetPeriodUseCase _FinishBudgetPeriodUseCase;
        private readonly ILogger<BudgetController> _Logger;
        public BudgetController(ICreateBudgetUseCase createBudgetUseCase, IGetAllBudgetsUseCase getAllBudgetsUseCase, IDeleteBudgetUseCase deleteBudgetUseCase, IFinishBudgetPeriodUseCase finishBudgetPeriodUseCase, ILogger<BudgetController> logger)
        {
            _CreateBudgetUseCase = createBudgetUseCase;
            _GetAllBudgetsUseCase = getAllBudgetsUseCase;
            _DeleteBudgetUseCase = deleteBudgetUseCase;
            _FinishBudgetPeriodUseCase = finishBudgetPeriodUseCase;
            _Logger = logger;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllForUser()
        {
            var result = await _GetAllBudgetsUseCase.InvokeAsync();
            if (!result.Successful)
            {
                _Logger.LogWarning("GetAllBudgets failed: {Error}", result.ErrorMessage);
                return BadRequest(result.ErrorMessage);
            }
            return Ok(result.Data);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] BudgetDto budgetDto)
        {
            var result = await _CreateBudgetUseCase.InvokeAsync(budgetDto);
            if (!result.Successful)
            {
                _Logger.LogWarning("CreateBudget failed: {Error}", result.ErrorMessage);
                return BadRequest(result.ErrorMessage);
            }
            return Ok(result.Data);
        }

        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> Delete([FromQuery] int budgetId)
        {
            var result = await _DeleteBudgetUseCase.InvokeAsync(budgetId);
            if (!result.Successful)
            {
                _Logger.LogWarning("DeleteBudget failed for budget {BudgetId}: {Error}", budgetId, result.ErrorMessage);
                return BadRequest(result.ErrorMessage);
            }
            return Ok(result.Data);
        }

        [Authorize]
        [HttpPost("finish")]
        public async Task<IActionResult> FinishPeriod([FromBody] FinishPeriodRequest request)
        {
            var result = await _FinishBudgetPeriodUseCase.InvokeAsync(request.Budget);
            if (!result.Successful)
            {
                _Logger.LogWarning("FinishBudgetPeriod failed: {Error}", result.ErrorMessage);
                return BadRequest(result.ErrorMessage);
            }
            return Ok(result.Data);
        }
    }
}
