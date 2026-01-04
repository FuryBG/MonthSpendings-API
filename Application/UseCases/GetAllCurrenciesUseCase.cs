using Application.Contracts;
using Application.Dto.Budget;
using Application.Interfaces;
using Application.Mappers;
using Domain;

namespace Application.UseCases
{
    public interface IGetAllCurrenciesUseCase
    {
        Task<CaseResult<List<CurrencyDto>>> InvokeAsync();
    }

    public class GetAllCurrenciesUseCase : IGetAllCurrenciesUseCase
    {
        private IUnitOfWork _UnitOfWork { get; set; }
        public GetAllCurrenciesUseCase(IUnitOfWork unitOfWork)
        {
            _UnitOfWork = unitOfWork;
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
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                result.Successful = false;
                result.ErrorMessage = "Something got wrong getting all currencies. Please try again later.";
            }
            return result;
        }
    }
}
