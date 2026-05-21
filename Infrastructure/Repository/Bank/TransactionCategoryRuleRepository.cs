using Application.Interfaces.Repository.Bank;
using Domain.Bank;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository.Bank
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
