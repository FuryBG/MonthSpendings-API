using Application.Dto.Savings;
using Application.UseCases.Savings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MonthSpendings.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SavingsPotInviteController : ControllerBase
    {
        private ISendSavingsPotInviteUseCase _SendInviteUseCase;
        private IUpdateSavingsPotInviteResponseUseCase _RespondUseCase;
        private readonly ILogger<SavingsPotInviteController> _Logger;

        public SavingsPotInviteController(
            ISendSavingsPotInviteUseCase sendInviteUseCase,
            IUpdateSavingsPotInviteResponseUseCase respondUseCase,
            ILogger<SavingsPotInviteController> logger)
        {
            _SendInviteUseCase = sendInviteUseCase;
            _RespondUseCase = respondUseCase;
            _Logger = logger;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Send([FromBody] SavingsPotInviteDto dto)
        {
            var result = await _SendInviteUseCase.InvokeAsync(dto);
            if (!result.Successful)
            {
                _Logger.LogWarning("SendSavingsPotInvite failed: {Error}", result.ErrorMessage);
                return BadRequest(result.ErrorMessage);
            }
            return Ok(result.Data);
        }

        [Authorize]
        [HttpPatch("{inviteId}")]
        public async Task<IActionResult> Respond(int inviteId, [FromBody] bool accepted)
        {
            var result = await _RespondUseCase.InvokeAsync(inviteId, accepted);
            if (!result.Successful)
            {
                _Logger.LogWarning("UpdateSavingsPotInviteResponse failed for invite {InviteId}: {Error}", inviteId, result.ErrorMessage);
                return BadRequest(result.ErrorMessage);
            }
            return Ok(result.Data);
        }
    }
}
