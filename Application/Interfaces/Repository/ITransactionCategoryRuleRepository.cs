using Domain.Bank;

namespace Application.Interfaces.Repository
{
    public interface ITransactionCategoryRuleRepository
    {
        Task<List<TransactionCategoryRule>> GetByUserIdAsync(int userId, CancellationToken cancellationToken);
        Task<TransactionCategoryRule?> FindMatchingRuleAsync(int userId, string merchantName, CancellationToken cancellationToken);
        Task<TransactionCategoryRule> AddAsync(TransactionCategoryRule rule, CancellationToken cancellationToken);
        Task<int> DeleteByIdAsync(int ruleId, int userId, CancellationToken cancellationToken);
        Task<int> DeleteByCategoryIdAsync(int categoryId, CancellationToken cancellationToken);
    }
}
