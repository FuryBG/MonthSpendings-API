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
            return result.Successful ? Ok(result.Data) : BadRequest(result.ErrorMessage);
        }
    }
}
