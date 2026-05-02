using Application.Dto;
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
        private IUpdateBudgetInviteResponseUseCase _UpdateBudgetInviteResponseUseCase;
        private readonly ILogger<BudgetInviteController> _Logger;
        public BudgetInviteController(ICreateBudgetInviteUseCase createBudgetInviteUseCase, IUpdateBudgetInviteResponseUseCase updateBudgetInviteResponseUseCase, ILogger<BudgetInviteController> logger)
        {
            _CreateBudgetInviteUseCase = createBudgetInviteUseCase;
            _UpdateBudgetInviteResponseUseCase = updateBudgetInviteResponseUseCase;
            _Logger = logger;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create(BudgetInviteDto budgetInviteDto)
        {
            var result = await _CreateBudgetInviteUseCase.InvokeAsync(budgetInviteDto);
            if (!result.Successful)
            {
                _Logger.LogWarning("CreateBudgetInvite failed: {Error}", result.ErrorMessage);
                return BadRequest(result.ErrorMessage);
            }
            return Ok(result.Data);
        }

        [Authorize]
        [HttpPatch("{inviteId}")]
        public async Task<IActionResult> Update(int inviteId, [FromBody] bool response)
        {
            var result = await _UpdateBudgetInviteResponseUseCase.InvokeAsync(inviteId, response);
            if (!result.Successful)
            {
                _Logger.LogWarning("UpdateBudgetInviteResponse failed for invite {InviteId}: {Error}", inviteId, result.ErrorMessage);
                return BadRequest(result.ErrorMessage);
            }
            return Ok(result.Data);
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
