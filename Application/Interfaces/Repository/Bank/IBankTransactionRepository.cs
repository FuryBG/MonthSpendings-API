using Domain.Bank;

namespace Application.Interfaces.Repository.Bank
{
    public interface IBankTransactionRepository
    {
        Task<BankTransaction> AddTransaction(BankTransaction bankTransaction, CancellationToken cancellationToken);
        Task<List<BankTransaction>> GetUncategorizedTransactionsByUser(int userId, CancellationToken cancellationToken);
        Task<BankTransaction?> GetTransactionById(int transactionId, int userId, CancellationToken cancellationToken);
        Task<int> CategorizeAsync(List<int> transactionIds, int spendingId, CancellationToken cancellationToken);
    }
}
