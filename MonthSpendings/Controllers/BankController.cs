using Application.UseCases.Bank;
using EnableBanking.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MonthSpendings.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BankController : ControllerBase
    {
        private IFinishBankConnectionUseCase _FinishBankConnectionUseCase;
        private IStartBankConnectionUseCase _StartBankConnectionUseCase;
        private IRemoveConnectedBankBySessionIdUseCase _RemoveBankConnectionBySessionIdUseCase;
        private IGetBanksUseCase _GetBanksUseCase;
        private IGetConnectedBanksByUserUseCase _GetConnectedBanksByUserUseCase;
        private readonly ILogger<BankController> _Logger;
        public BankController(IGetBanksUseCase getBanksUseCase, IStartBankConnectionUseCase startBankConnectionUseCase, IRemoveConnectedBankBySessionIdUseCase removeConnectedBankBySessionIdUseCase, IFinishBankConnectionUseCase finishBankConnectionUseCase, IGetConnectedBanksByUserUseCase getConnectedBanksByUserUseCase, ISessionsService sessionsService, ILogger<BankController> logger)
        {
            _StartBankConnectionUseCase = startBankConnectionUseCase;
            _FinishBankConnectionUseCase = finishBankConnectionUseCase;
            _RemoveBankConnectionBySessionIdUseCase = removeConnectedBankBySessionIdUseCase;
            _GetBanksUseCase = getBanksUseCase;
            _GetConnectedBanksByUserUseCase = getConnectedBanksByUserUseCase;
            _Logger = logger;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetBanks(string? bankName, CancellationToken cancellationToken)
        {
            var result = await _GetBanksUseCase.InvokeAsync(bankName, cancellationToken);

            if (result.Successful)
            {
                return Ok(result.Data);
            }
            else
            {
                _Logger.LogWarning("GetBanks failed: {Error}", result.ErrorMessage);
                return BadRequest(result.ErrorMessage);
            }
        }

        [Authorize]
        [HttpGet("connected")]
        public async Task<IActionResult> GetConnectedBanks()
        {
            var result = await _GetConnectedBanksByUserUseCase.InvokeAsync();
            if (result.Successful)
            {
                return Ok(result.Data);
            }
            else
            {
                _Logger.LogWarning("GetConnectedBanks failed: {Error}", result.ErrorMessage);
                return BadRequest(result.ErrorMessage);
            }
        }

        [Authorize]
        [HttpGet("connect")]
        public async Task<IActionResult> Connect(string bankName, string countryCode, string bankImageUrl, int maximumConsentValidity)
        {
            var result = await _StartBankConnectionUseCase.InvokeAsync(bankName, countryCode, bankImageUrl, maximumConsentValidity);

            if (result.Successful)
            {
                return Ok(result.Data);
            }
            else
            {
                _Logger.LogWarning("StartBankConnection failed: {Error}", result.ErrorMessage);
                return BadRequest(result.ErrorMessage);
            }
        }

        [Authorize]
        [HttpDelete("delete")]
        public async Task<IActionResult> Delete(Guid sessionId, CancellationToken cancellationToken)
        {
            var result = await _RemoveBankConnectionBySessionIdUseCase.InvokeAsync(sessionId, cancellationToken);
            if (result.Successful)
            {
                return Ok(result.Data);
            }
            else
            {
                _Logger.LogWarning("RemoveBankConnection failed: {Error}", result.ErrorMessage);
                return BadRequest(result.ErrorMessage);
            }
        }

        [HttpGet("connect-callback")]
        public async Task<IActionResult> ConnectCallback(Guid state)
        {
            // Read "code" from the raw query string instead of relying on model binding:
            // ASP.NET Core's query parser decodes '+' as a space (form-encoding convention),
            // which corrupts EnableBanking authorization codes that contain a literal '+'.
            string code = GetRawQueryParam("code") ?? string.Empty;

            var result = await _FinishBankConnectionUseCase.InvokeAsync(state, code);

            if (result.Successful)
            {
                return Redirect(result.Data);
            }
            else
            {
                _Logger.LogWarning("FinishBankConnection failed for state {State}", state);
                return Redirect(result.Data!);
            }
        }

        private string? GetRawQueryParam(string key)
        {
            string raw = Request.QueryString.Value?.TrimStart('?') ?? string.Empty;

            foreach (var pair in raw.Split('&'))
            {
                int separatorIndex = pair.IndexOf('=');
                if (separatorIndex < 0)
                {
                    continue;
                }

                if (pair[..separatorIndex] == key)
                {
                    return Uri.UnescapeDataString(pair[(separatorIndex + 1)..]);
                }
            }

            return null;
        }
    }
}