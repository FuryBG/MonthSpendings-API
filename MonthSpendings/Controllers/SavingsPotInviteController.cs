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

        public SavingsPotInviteController(
            ISendSavingsPotInviteUseCase sendInviteUseCase,
            IUpdateSavingsPotInviteResponseUseCase respondUseCase)
        {
            _SendInviteUseCase = sendInviteUseCase;
            _RespondUseCase = respondUseCase;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Send([FromBody] SavingsPotInviteDto dto)
        {
            var result = await _SendInviteUseCase.InvokeAsync(dto);
            return result.Successful ? Ok(result.Data) : BadRequest(result.ErrorMessage);
        }

        [Authorize]
        [HttpPatch("{inviteId}")]
        public async Task<IActionResult> Respond(int inviteId, [FromBody] bool accepted)
        {
            var result = await _RespondUseCase.InvokeAsync(inviteId, accepted);
            return result.Successful ? Ok(result.Data) : BadRequest(result.ErrorMessage);
        }
    }
}
