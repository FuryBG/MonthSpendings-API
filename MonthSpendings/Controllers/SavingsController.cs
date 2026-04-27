using Application.Dto.Savings;
using Application.UseCases.Savings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MonthSpendings.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SavingsController : ControllerBase
    {
        private IGetAllSavingsPotsUseCase _GetAllUseCase;
        private ICreateSavingsPotUseCase _CreateUseCase;
        private IDeleteSavingsPotUseCase _DeleteUseCase;
        private IAddSavingsContributionUseCase _AddContributionUseCase;
        private IRemoveSavingsContributionUseCase _RemoveContributionUseCase;
        private IGetSavingsHistoryUseCase _GetHistoryUseCase;

        public SavingsController(
            IGetAllSavingsPotsUseCase getAllUseCase,
            ICreateSavingsPotUseCase createUseCase,
            IDeleteSavingsPotUseCase deleteUseCase,
            IAddSavingsContributionUseCase addContributionUseCase,
            IRemoveSavingsContributionUseCase removeContributionUseCase,
            IGetSavingsHistoryUseCase getHistoryUseCase)
        {
            _GetAllUseCase = getAllUseCase;
            _CreateUseCase = createUseCase;
            _DeleteUseCase = deleteUseCase;
            _AddContributionUseCase = addContributionUseCase;
            _RemoveContributionUseCase = removeContributionUseCase;
            _GetHistoryUseCase = getHistoryUseCase;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _GetAllUseCase.InvokeAsync();
            return result.Successful ? Ok(result.Data) : BadRequest(result.ErrorMessage);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SavingsPotDto dto)
        {
            var result = await _CreateUseCase.InvokeAsync(dto);
            return result.Successful ? Ok(result.Data) : BadRequest(result.ErrorMessage);
        }

        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> Delete([FromQuery] int potId)
        {
            var result = await _DeleteUseCase.InvokeAsync(potId);
            return result.Successful ? Ok(result.Data) : BadRequest(result.ErrorMessage);
        }

        [Authorize]
        [HttpPost("{potId}/contribution")]
        public async Task<IActionResult> AddContribution(int potId, [FromBody] SavingsContributionDto dto)
        {
            var result = await _AddContributionUseCase.InvokeAsync(potId, dto);
            return result.Successful ? Ok(result.Data) : BadRequest(result.ErrorMessage);
        }

        [Authorize]
        [HttpDelete("{potId}/contribution")]
        public async Task<IActionResult> RemoveContribution(int potId, [FromQuery] int contributionId)
        {
            var result = await _RemoveContributionUseCase.InvokeAsync(potId, contributionId);
            return result.Successful ? Ok(result.Data) : BadRequest(result.ErrorMessage);
        }

        [Authorize]
        [HttpGet("{potId}/history")]
        public async Task<IActionResult> GetHistory(int potId)
        {
            var result = await _GetHistoryUseCase.InvokeAsync(potId);
            return result.Successful ? Ok(result.Data) : BadRequest(result.ErrorMessage);
        }
    }
}
