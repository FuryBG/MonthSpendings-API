using Application.Dto;
using Application.UseCases;
using Microsoft.AspNetCore.Mvc;
using MonthSpendings.Filters;

namespace MonthSpendings.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RevenueCatController : ControllerBase
    {
        private readonly IHandleRevenueCatWebhookUseCase _HandleRevenueCatWebhookUseCase;
        private readonly ILogger<RevenueCatController> _Logger;

        public RevenueCatController(
            IHandleRevenueCatWebhookUseCase handleRevenueCatWebhookUseCase,
            ILogger<RevenueCatController> logger)
        {
            _HandleRevenueCatWebhookUseCase = handleRevenueCatWebhookUseCase;
            _Logger = logger;
        }

        [HttpPost("webhook")]
        [RequireRevenueCatSecret]
        public async Task<IActionResult> Webhook([FromBody] RevenueCatWebhookPayload payload)
        {
            var result = await _HandleRevenueCatWebhookUseCase.InvokeAsync(payload);

            if (!result.Successful)
            {
                _Logger.LogError("RevenueCat webhook: processing failed: {Error}", result.ErrorMessage);
                return StatusCode(500);
            }

            return Ok();
        }
    }
}
