using Domain;

namespace Application.Interfaces.Repository
{
    public interface ICurrencyRepository
    {
        Task<List<Currency>> GetAllCurrencies();
    }
}