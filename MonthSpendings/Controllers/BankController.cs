using Application.Contracts;
using Application.Services;
using Application.UseCases.Bank;
using EnableBanking.Interfaces;
using EnableBanking.Models;
using EnableBanking.Models.General;
using EnableBanking.Models.Sessions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MonthSpendings.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BankController : ControllerBase
    {
        private IFinishBankConnectionUseCase _FinishBankConnectionUseCase;
        private IStartBankConnectionUseCase _StartBankConnectionUseCase;
        private IGetBanksUseCase _GetBanksUseCase;
        public BankController(IGetBanksUseCase getBanksUseCase, IStartBankConnectionUseCase startBankConnectionUseCase, IFinishBankConnectionUseCase finishBankConnectionUseCase, ISessionsService sessionsService)
        {
            _StartBankConnectionUseCase = startBankConnectionUseCase;
            _FinishBankConnectionUseCase = finishBankConnectionUseCase;
            _GetBanksUseCase = getBanksUseCase;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetBanks(string? bankName)
        {
            var result = await _GetBanksUseCase.InvokeAsync(bankName);

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
        [HttpGet("connect")]
        public async Task<IActionResult> Connect(string bankName, string countryCode, string bankImgUrl, int maximumConsentValidity)
        {
            var result = await _StartBankConnectionUseCase.InvokeAsync(bankName, countryCode, bankImgUrl, maximumConsentValidity);

            if (result.Successful)
            {
                return Ok(result.Data);
            }
            else
            {
                return BadRequest(result.ErrorMessage);
            }
        }

        [HttpGet("connect-callback")]
        public async Task<IActionResult> ConnectCallback(Guid sessionId, string code)
        {
            var result = await _FinishBankConnectionUseCase.InvokeAsync(sessionId, code);

            if (result.Successful)
            {
                return Redirect(result.Data);
            }
            else
            {
                return Redirect(result.Data!);
            }
        }
    }
}