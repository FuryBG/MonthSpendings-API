using Application.UseCases.SaltEdge;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SaltEdge.Models.Callbacks;

namespace MonthSpendings.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SaltEdgeBankController : ControllerBase
    {
        private readonly IGetSaltEdgeBanksUseCase _GetSaltEdgeBanksUseCase;
        private readonly IGetConnectedSaltEdgeBanksByUserUseCase _GetConnectedSaltEdgeBanksByUserUseCase;
        private readonly IGetSaltEdgeConnectionStatusUseCase _GetSaltEdgeConnectionStatusUseCase;
        private readonly IStartSaltEdgeConnectionUseCase _StartSaltEdgeConnectionUseCase;
        private readonly IFinishSaltEdgeConnectionUseCase _FinishSaltEdgeConnectionUseCase;
        private readonly IFailSaltEdgeConnectionUseCase _FailSaltEdgeConnectionUseCase;
        private readonly IRemoveSaltEdgeConnectionUseCase _RemoveSaltEdgeConnectionUseCase;
        private readonly IConfiguration _Configuration;
        private readonly ILogger<SaltEdgeBankController> _Logger;

        public SaltEdgeBankController(
            IGetSaltEdgeBanksUseCase getSaltEdgeBanksUseCase,
            IGetConnectedSaltEdgeBanksByUserUseCase getConnectedSaltEdgeBanksByUserUseCase,
            IGetSaltEdgeConnectionStatusUseCase getSaltEdgeConnectionStatusUseCase,
            IStartSaltEdgeConnectionUseCase startSaltEdgeConnectionUseCase,
            IFinishSaltEdgeConnectionUseCase finishSaltEdgeConnectionUseCase,
            IFailSaltEdgeConnectionUseCase failSaltEdgeConnectionUseCase,
            IRemoveSaltEdgeConnectionUseCase removeSaltEdgeConnectionUseCase,
            IConfiguration configuration,
            ILogger<SaltEdgeBankController> logger)
        {
            _GetSaltEdgeBanksUseCase = getSaltEdgeBanksUseCase;
            _GetConnectedSaltEdgeBanksByUserUseCase = getConnectedSaltEdgeBanksByUserUseCase;
            _GetSaltEdgeConnectionStatusUseCase = getSaltEdgeConnectionStatusUseCase;
            _StartSaltEdgeConnectionUseCase = startSaltEdgeConnectionUseCase;
            _FinishSaltEdgeConnectionUseCase = finishSaltEdgeConnectionUseCase;
            _FailSaltEdgeConnectionUseCase = failSaltEdgeConnectionUseCase;
            _RemoveSaltEdgeConnectionUseCase = removeSaltEdgeConnectionUseCase;
            _Configuration = configuration;
            _Logger = logger;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetBanks(string? bankName, CancellationToken cancellationToken)
        {
            var result = await _GetSaltEdgeBanksUseCase.InvokeAsync(bankName, cancellationToken);
            if (result.Successful) return Ok(result.Data);
            _Logger.LogWarning("GetSaltEdgeBanks failed: {Error}", result.ErrorMessage);
            return BadRequest(result.ErrorMessage);
        }

        [Authorize]
        [HttpGet("connected")]
        public async Task<IActionResult> GetConnectedBanks(CancellationToken cancellationToken)
        {
            var result = await _GetConnectedSaltEdgeBanksByUserUseCase.InvokeAsync(cancellationToken);
            if (result.Successful) return Ok(result.Data);
            _Logger.LogWarning("GetConnectedSaltEdgeBanks failed: {Error}", result.ErrorMessage);
            return BadRequest(result.ErrorMessage);
        }

        [Authorize]
        [HttpGet("connect-status")]
        public async Task<IActionResult> GetConnectionStatus(Guid localSessionId, CancellationToken cancellationToken)
        {
            var result = await _GetSaltEdgeConnectionStatusUseCase.InvokeAsync(localSessionId, cancellationToken);
            if (result.Successful) return Ok(result.Data);
            _Logger.LogWarning("GetSaltEdgeConnectionStatus failed: {Error}", result.ErrorMessage);
            return BadRequest(result.ErrorMessage);
        }

        [Authorize]
        [HttpGet("connect")]
        public async Task<IActionResult> Connect(string providerCode, string providerName, string countryCode, string bankImageUrl, int consentDays = 90, CancellationToken cancellationToken = default)
        {
            var result = await _StartSaltEdgeConnectionUseCase.InvokeAsync(providerCode, providerName, countryCode, bankImageUrl, consentDays, cancellationToken);
            if (result.Successful) return Ok(result.Data);
            _Logger.LogWarning("StartSaltEdgeConnection failed: {Error}", result.ErrorMessage);
            return BadRequest(result.ErrorMessage);
        }

        [HttpGet("connect-callback")]
        public IActionResult ConnectCallback()
        {
            string redirectUrl = _Configuration["SaltEdge:ConnectReturnUrl"] ?? "monthspendings://(main)/ConnectSaltEdgePending";
            return Redirect(redirectUrl);
        }

        [HttpPost("callback/success")]
        public async Task<IActionResult> SuccessCallback([FromBody] SaltEdgeCallbackRequest request, CancellationToken cancellationToken)
        {
            if (!TryGetLocalSessionId(request, out Guid localSessionId) || string.IsNullOrWhiteSpace(request.Data?.ConnectionId))
            {
                return BadRequest();
            }

            _Logger.LogInformation("SaltEdge success callback received for localSessionId {LocalSessionId}", localSessionId);
            var result = await _FinishSaltEdgeConnectionUseCase.InvokeAsync(request.Data.ConnectionId, localSessionId, request.Data.Stage, cancellationToken);
            if (result.Successful) return Ok();
            _Logger.LogWarning("FinishSaltEdgeConnection failed for localSessionId {LocalSessionId}: {Error}", localSessionId, result.ErrorMessage);
            return BadRequest(result.ErrorMessage);
        }

        [HttpPost("callback/fail")]
        public async Task<IActionResult> FailCallback([FromBody] SaltEdgeCallbackRequest request, CancellationToken cancellationToken)
        {
            _Logger.LogInformation("SaltEdge fail callback received");
            if (!TryGetLocalSessionId(request, out Guid localSessionId))
            {
                return BadRequest();
            }

            var result = await _FailSaltEdgeConnectionUseCase.InvokeAsync(localSessionId, request.Data?.ConnectionId, request.Data?.ErrorClass, request.Data?.ErrorMessage, cancellationToken);
            if (result.Successful) return Ok();
            _Logger.LogWarning("FailSaltEdgeConnection failed for localSessionId {LocalSessionId}: {Error}", localSessionId, result.ErrorMessage);
            return BadRequest(result.ErrorMessage);
        }

        [Authorize]
        [HttpDelete("delete")]
        public async Task<IActionResult> Delete(int connectionDbId, CancellationToken cancellationToken)
        {
            var result = await _RemoveSaltEdgeConnectionUseCase.InvokeAsync(connectionDbId, cancellationToken);
            if (result.Successful) return Ok(result.Data);
            _Logger.LogWarning("RemoveSaltEdgeConnection failed: {Error}", result.ErrorMessage);
            return BadRequest(result.ErrorMessage);
        }

        private static bool TryGetLocalSessionId(SaltEdgeCallbackRequest request, out Guid localSessionId)
        {
            localSessionId = Guid.Empty;
            return request.Data?.CustomFields != null
                && request.Data.CustomFields.TryGetValue("local_session_id", out string? localSessionIdValue)
                && Guid.TryParse(localSessionIdValue, out localSessionId);
        }
    }
}
