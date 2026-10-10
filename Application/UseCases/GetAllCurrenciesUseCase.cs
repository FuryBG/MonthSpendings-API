using Application.Resources;
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

            List<Currency> currencies = await _UnitOfWork.CurrencyRepository.GetAllCurrencies();
            List<CurrencyDto> currenciesDto = currencies.Select(currency => currency.ToDto()).ToList();
            result.Data = currenciesDto;
            _Logger.LogDebug("Retrieved {Count} currencies", currenciesDto.Count);
            return result;
        }
    }
}
