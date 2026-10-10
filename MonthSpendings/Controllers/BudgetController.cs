using Application.Dto.Budget;
using Application.UseCases;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MonthSpendings.Contracts.Requests;

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
        private IKickBudgetMemberUseCase _KickBudgetMemberUseCase;
        private ILeaveBudgetUseCase _LeaveBudgetUseCase;
        public BudgetController(ICreateBudgetUseCase createBudgetUseCase, IGetAllBudgetsUseCase getAllBudgetsUseCase, IDeleteBudgetUseCase deleteBudgetUseCase, IFinishBudgetPeriodUseCase finishBudgetPeriodUseCase, IKickBudgetMemberUseCase kickBudgetMemberUseCase, ILeaveBudgetUseCase leaveBudgetUseCase)
        {
            _CreateBudgetUseCase = createBudgetUseCase;
            _GetAllBudgetsUseCase = getAllBudgetsUseCase;
            _DeleteBudgetUseCase = deleteBudgetUseCase;
            _FinishBudgetPeriodUseCase = finishBudgetPeriodUseCase;
            _KickBudgetMemberUseCase = kickBudgetMemberUseCase;
            _LeaveBudgetUseCase = leaveBudgetUseCase;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAllForUser()
        {
            var result = await _GetAllBudgetsUseCase.InvokeAsync();
            if (!result.Successful)
            {
                return StatusCode((int)result.ErrorType, result.ErrorMessage);
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
                return StatusCode((int)result.ErrorType, result.ErrorMessage);
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
                return StatusCode((int)result.ErrorType, result.ErrorMessage);
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
                return StatusCode((int)result.ErrorType, result.ErrorMessage);
            }
            return Ok(result.Data);
        }

        [Authorize]
        [HttpDelete("{budgetId}/members/{userId}")]
        public async Task<IActionResult> KickMember(int budgetId, int userId)
        {
            var result = await _KickBudgetMemberUseCase.InvokeAsync(budgetId, userId);
            if (!result.Successful)
            {
                return StatusCode((int)result.ErrorType, result.ErrorMessage);
            }
            return NoContent();
        }

        [Authorize]
        [HttpDelete("{budgetId}/leave")]
        public async Task<IActionResult> Leave(int budgetId)
        {
            var result = await _LeaveBudgetUseCase.InvokeAsync(budgetId);
            if (!result.Successful)
            {
                return StatusCode((int)result.ErrorType, result.ErrorMessage);
            }
            return NoContent();
        }
    }
}
