using Domain.Bank;
using System.Linq.Expressions;

namespace Application.Interfaces.Repository.Bank
{
    public interface IBankAccountRepository
    {
        Task<List<BankAccount>> GetAllAsync(Expression<Func<BankAccount, bool>> predicate);
        Task<BankAccount?> GetAsync(Expression<Func<BankAccount, bool>> predicate);
    }
}