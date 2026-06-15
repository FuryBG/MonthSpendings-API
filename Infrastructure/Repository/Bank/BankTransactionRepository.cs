using Application.Interfaces.Repository.Bank;
using Domain.Bank;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository.Bank
{
    public class BankTransactionRepository : IBankTransactionRepository
    {
        private AppDbContext _DbContext { get; set; }
        public BankTransactionRepository(AppDbContext dbContext)
        {
            _DbContext = dbContext;
        }

        public async Task<BankTransaction> AddTransaction(BankTransaction bankTransaction, CancellationToken cancellationToken)
        {
            await _DbContext.BankTransactions.AddAsync(bankTransaction);
            return bankTransaction;
        }

        public async Task<List<BankTransaction>> GetUncategorizedTransactionsByUser(int userId, CancellationToken cancellationToken)
        {
            return await _DbContext.BankTransactions
                .Include(transaction => transaction.BankAccount)
                .ThenInclude(account => account!.Consent)
                .Where(transaction => !transaction.Categorized
                    && transaction.BankAccount != null
                    && transaction.BankAccount.Consent != null
                    && transaction.BankAccount.Consent.UserId == userId)
                .OrderByDescending(transaction => transaction.BookingDate)
                .ToListAsync(cancellationToken);
        }

        public async Task<BankTransaction?> GetTransactionById(int transactionId, int userId, CancellationToken cancellationToken)
        {
            return await _DbContext.BankTransactions
                .Include(transaction => transaction.BankAccount)
                .ThenInclude(account => account!.Consent)
                .FirstOrDefaultAsync(transaction => transaction.Id == transactionId
                    && transaction.BankAccount != null
                    && transaction.BankAccount.Consent != null
                    && transaction.BankAccount.Consent.UserId == userId, cancellationToken);
        }

        public Task<BankTransaction?> GetTransactionByTransactionId(string transactionId, CancellationToken cancellationToken)
        {
            return _DbContext.BankTransactions
                .FirstOrDefaultAsync(t => t.TransactionId == transactionId, cancellationToken);
        }

        public async Task<HashSet<string>> GetExistingTransactionIdsAsync(IEnumerable<string> transactionIds, CancellationToken cancellationToken)
        {
            List<string> ids = transactionIds.ToList();
            List<string> found = await _DbContext.BankTransactions
                .Where(t => ids.Contains(t.TransactionId))
                .Select(t => t.TransactionId)
                .ToListAsync(cancellationToken);

            return found.ToHashSet();
        }

        public Task<int> CategorizeAsync(List<int> transactionIds, int spendingId, CancellationToken cancellationToken)
        {
            return _DbContext.BankTransactions
                .Where(t => transactionIds.Contains(t.Id))
                .ExecuteUpdateAsync(c => c.SetProperty(t => t.Categorized, _ => true)
                                        .SetProperty(t => t.SpendingId, _ => spendingId), cancellationToken);
        }
    }
}