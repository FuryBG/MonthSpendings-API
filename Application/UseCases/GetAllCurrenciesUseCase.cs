using Application.Contracts;
using Application.Dto.Budget;
using Application.Interfaces;
using Application.Mappers;
using Domain;
using Microsoft.Extensions.Logging;

namespace Application.UseCases
{
    public interface IGetAllCurrenciesUseCase
    {
        Task<CaseResult<List<CurrencyDto>>> InvokeAsync();
    }

    public class GetAllCurrenciesUseCase : IGetAllCurrenciesUseCase
    {
        private IUnitOfWork _UnitOfWork { get; set; }
        private readonly ILogger<GetAllCurrenciesUseCase> _Logger;
        public GetAllCurrenciesUseCase(IUnitOfWork unitOfWork, ILogger<GetAllCurrenciesUseCase> logger)
        {
            _UnitOfWork = unitOfWork;
            _Logger = logger;
        }

        public async Task<CaseResult<List<CurrencyDto>>> InvokeAsync()
        {
            var result = new CaseResult<List<CurrencyDto>>();
            result.Successful = true;

            try
            {
                List<Currency> currencies = await _UnitOfWork.CurrencyRepository.GetAllCurrencies();
                List<CurrencyDto> currenciesDto = currencies.Select(currency => currency.ToDto()).ToList();
                result.Data = currenciesDto;
                _Logger.LogInformation("Retrieved {Count} currencies", currenciesDto.Count);
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "Error retrieving currencies");
                result.Successful = false;
                result.ErrorMessage = "Something got wrong getting all currencies. Please try again later.";
            }
            return result;
        }
    }
}
