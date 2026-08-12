using Application.Interfaces.Repository;
using Domain.Bank;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository
{
    public class TransactionCategoryRuleRepository : ITransactionCategoryRuleRepository
    {
        private readonly AppDbContext _DbContext;

        public TransactionCategoryRuleRepository(AppDbContext dbContext)
        {
            _DbContext = dbContext;
        }

        public Task<List<TransactionCategoryRule>> GetByUserIdAsync(int userId, CancellationToken cancellationToken)
        {
            return _DbContext.TransactionCategoryRules
                .Where(r => r.UserId == userId)
                .ToListAsync(cancellationToken);
        }

        public Task<TransactionCategoryRule?> FindMatchingRuleAsync(int userId, string merchantName, CancellationToken cancellationToken)
        {
            string lower = merchantName.ToLowerInvariant();
            return _DbContext.TransactionCategoryRules
                .Where(r => r.UserId == userId && lower.Contains(r.Keyword.ToLower()))
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<TransactionCategoryRule> AddAsync(TransactionCategoryRule rule, CancellationToken cancellationToken)
        {
            await _DbContext.TransactionCategoryRules.AddAsync(rule, cancellationToken);
            return rule;
        }

        public Task<int> DeleteByIdAsync(int ruleId, int userId, CancellationToken cancellationToken)
        {
            return _DbContext.TransactionCategoryRules
                .Where(r => r.Id == ruleId && r.UserId == userId)
                .ExecuteDeleteAsync(cancellationToken);
        }

        public Task<int> DeleteByCategoryIdAsync(int categoryId, CancellationToken cancellationToken)
        {
            return _DbContext.TransactionCategoryRules
                .Where(r => r.CategoryId == categoryId)
                .ExecuteDeleteAsync(cancellationToken);
        }
    }
}
