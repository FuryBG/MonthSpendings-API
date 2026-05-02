using Application.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace MonthSpendings.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CurrencyController : ControllerBase
    {
        private IGetAllCurrenciesUseCase _GetAllCurrenciesUseCase { get; set; }
        private readonly ILogger<CurrencyController> _Logger;
        public CurrencyController(IGetAllCurrenciesUseCase getAllCurrenciesUseCase, ILogger<CurrencyController> logger)
        {
            _GetAllCurrenciesUseCase = getAllCurrenciesUseCase;
            _Logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCurrencies()
        {
            var result = await _GetAllCurrenciesUseCase.InvokeAsync();
            if (!result.Successful)
            {
                _Logger.LogWarning("GetAllCurrencies failed: {Error}", result.ErrorMessage);
                return BadRequest(result.ErrorMessage);
            }
            return Ok(result.Data);
        }
    }
}
