using Application.Interfaces.Repository;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository
{
    public class CurrencyRepository : ICurrencyRepository
    {
        private AppDbContext _DbContext { get; set; }
        public CurrencyRepository(AppDbContext dbContext)
        {
            _DbContext = dbContext;
        }

        public async Task<List<Currency>> GetAllCurrencies()
        {
            return await _DbContext.Currencies.ToListAsync();
        }
    }
}
