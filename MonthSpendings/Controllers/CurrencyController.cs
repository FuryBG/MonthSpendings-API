using Application.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace MonthSpendings.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CurrencyController : ControllerBase
    {
        private IGetAllCurrenciesUseCase _GetAllCurrenciesUseCase { get; set; }
        public CurrencyController(IGetAllCurrenciesUseCase getAllCurrenciesUseCase)
        {
            _GetAllCurrenciesUseCase = getAllCurrenciesUseCase;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCurrencies()
        {
            var result = await _GetAllCurrenciesUseCase.InvokeAsync();
            if (!result.Successful)
            {
                return StatusCode((int)result.ErrorType, result.ErrorMessage);
            }
            return Ok(result.Data);
        }
    }
}
