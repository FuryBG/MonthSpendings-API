using Application.Interfaces.Repository.Bank;
using Domain.Bank;

namespace Infrastructure.Repository.Bank
{
    public class BankTransactionRepository : IBankTransactionRepository
    {
        private AppDbContext _DbContext { get; set; }
        public BankTransactionRepository(AppDbContext dbContext)
        {
            _DbContext = dbContext;
        }

        public async Task<BankTransaction> AddTransaction(BankTransaction bankTransaction)
        {
            await _DbContext.BankTransactions.AddAsync(bankTransaction);
            return bankTransaction;
        }
    }
}
