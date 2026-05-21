using Domain.Bank;

namespace Application.Interfaces.Repository.Bank
{
    public interface ITransactionCategoryRuleRepository
    {
        Task<List<TransactionCategoryRule>> GetByUserIdAsync(int userId, CancellationToken cancellationToken);
        Task<TransactionCategoryRule> AddAsync(TransactionCategoryRule rule, CancellationToken cancellationToken);
        Task<int> DeleteByIdAsync(int ruleId, int userId, CancellationToken cancellationToken);
        Task<int> DeleteByCategoryIdAsync(int categoryId, CancellationToken cancellationToken);
    }
}
