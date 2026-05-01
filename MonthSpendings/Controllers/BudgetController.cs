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
        public BudgetController(ICreateBudgetUseCase createBudgetUseCase, IGetAllBudgetsUseCase getAllBudgetsUseCase, IDeleteBudgetUseCase deleteBudgetUseCase, IFinishBudgetPeriodUseCase finishBudgetPeriodUseCase)
        {
            _CreateBudgetUseCase = createBudgetUseCase;
            _GetAllBudgetsUseCase = getAllBudgetsUseCase;
            _DeleteBudgetUseCase = deleteBudgetUseCase;
            _FinishBudgetPeriodUseCase = finishBudgetPeriodUseCase;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllForUser()
        {
            var result = await _GetAllBudgetsUseCase.InvokeAsync();
            return result.Successful ? Ok(result.Data) : BadRequest(result.ErrorMessage);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] BudgetDto budgetDto)
        {
            Console.WriteLine(ModelState);
            var result = await _CreateBudgetUseCase.InvokeAsync(budgetDto);
            return result.Successful ? Ok(result.Data) : BadRequest(result.ErrorMessage);
        }

        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> Delete([FromQuery] int budgetId)
        {
            var result = await _DeleteBudgetUseCase.InvokeAsync(budgetId);
            return result.Successful ? Ok(result.Data) : BadRequest(result.ErrorMessage);
        }

        [Authorize]
        [HttpPost("finish")]
        public async Task<IActionResult> FinishPeriod([FromBody] FinishPeriodRequest request)
        {
            Console.WriteLine(ModelState);
            var result = await _FinishBudgetPeriodUseCase.InvokeAsync(request.Budget, request.SavingsPotId);
            return result.Successful ? Ok(result.Data) : BadRequest(result.ErrorMessage);
        }
    }
}
