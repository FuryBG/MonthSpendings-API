using Application.Interfaces.Repository.Bank;
using Domain.Bank;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Repository.Bank
{
    public class BankAccountRepository : IBankAccountRepository
    {
        private AppDbContext _DbContext { get; set; }
        public BankAccountRepository(AppDbContext dbContext)
        {
            _DbContext = dbContext;
        }

        public async Task<BankAccount?> GetAsync(Expression<Func<BankAccount, bool>> predicate)
        {
            return await _DbContext.BankAccounts.FirstOrDefaultAsync(predicate);
        }
        public async Task<List<BankAccount>> GetAllAsync(Expression<Func<BankAccount, bool>> predicate)
        {
            return await _DbContext.BankAccounts.Include(ba => ba.Consent).Where(predicate).ToListAsync();
        }
    }
}
