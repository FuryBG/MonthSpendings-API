using Domain.Bank;

namespace Application.Interfaces.Repository.Bank
{
    public interface IBankTransactionRepository
    {
        Task<BankTransaction> AddTransaction(BankTransaction bankTransaction);
    }
}