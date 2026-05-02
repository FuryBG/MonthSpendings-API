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
        private readonly ILogger<SavingsController> _Logger;

        public SavingsController(
            IGetAllSavingsPotsUseCase getAllUseCase,
            ICreateSavingsPotUseCase createUseCase,
            IDeleteSavingsPotUseCase deleteUseCase,
            IAddSavingsContributionUseCase addContributionUseCase,
            IRemoveSavingsContributionUseCase removeContributionUseCase,
            IGetSavingsHistoryUseCase getHistoryUseCase,
            ILogger<SavingsController> logger)
        {
            _GetAllUseCase = getAllUseCase;
            _CreateUseCase = createUseCase;
            _DeleteUseCase = deleteUseCase;
            _AddContributionUseCase = addContributionUseCase;
            _RemoveContributionUseCase = removeContributionUseCase;
            _GetHistoryUseCase = getHistoryUseCase;
            _Logger = logger;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _GetAllUseCase.InvokeAsync();
            if (!result.Successful)
            {
                _Logger.LogWarning("GetAllSavingsPots failed: {Error}", result.ErrorMessage);
                return BadRequest(result.ErrorMessage);
            }
            return Ok(result.Data);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SavingsPotDto dto)
        {
            var result = await _CreateUseCase.InvokeAsync(dto);
            if (!result.Successful)
            {
                _Logger.LogWarning("CreateSavingsPot failed: {Error}", result.ErrorMessage);
                return BadRequest(result.ErrorMessage);
            }
            return Ok(result.Data);
        }

        [Authorize]
        [HttpDelete]
        public async Task<IActionResult> Delete([FromQuery] int potId)
        {
            var result = await _DeleteUseCase.InvokeAsync(potId);
            if (!result.Successful)
            {
                _Logger.LogWarning("DeleteSavingsPot failed for pot {PotId}: {Error}", potId, result.ErrorMessage);
                return BadRequest(result.ErrorMessage);
            }
            return Ok(result.Data);
        }

        [Authorize]
        [HttpPost("{potId}/contribution")]
        public async Task<IActionResult> AddContribution(int potId, [FromBody] SavingsContributionDto dto)
        {
            var result = await _AddContributionUseCase.InvokeAsync(potId, dto);
            if (!result.Successful)
            {
                _Logger.LogWarning("AddSavingsContribution failed for pot {PotId}: {Error}", potId, result.ErrorMessage);
                return BadRequest(result.ErrorMessage);
            }
            return Ok(result.Data);
        }

        [Authorize]
        [HttpDelete("{potId}/contribution")]
        public async Task<IActionResult> RemoveContribution(int potId, [FromQuery] int contributionId)
        {
            var result = await _RemoveContributionUseCase.InvokeAsync(potId, contributionId);
            if (!result.Successful)
            {
                _Logger.LogWarning("RemoveSavingsContribution failed for pot {PotId}: {Error}", potId, result.ErrorMessage);
                return BadRequest(result.ErrorMessage);
            }
            return Ok(result.Data);
        }

        [Authorize]
        [HttpGet("{potId}/history")]
        public async Task<IActionResult> GetHistory(int potId)
        {
            var result = await _GetHistoryUseCase.InvokeAsync(potId);
            if (!result.Successful)
            {
                _Logger.LogWarning("GetSavingsHistory failed for pot {PotId}: {Error}", potId, result.ErrorMessage);
                return BadRequest(result.ErrorMessage);
            }
            return Ok(result.Data);
        }
    }
}
