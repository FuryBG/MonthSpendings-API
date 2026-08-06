using Application.Interfaces.Repository;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository
{
    public class CategorySpendingsRepository : ICategorySpendingsRepository
    {
        private AppDbContext _DbContext { get; set; }
        public CategorySpendingsRepository(AppDbContext dbContext)
        {
            _DbContext = dbContext;
        }

        public Spending AddSpending(Spending spending)
        {
            _DbContext.Spendings.Add(spending);
            return spending;
        }

        public async Task<Spending?> GetSpending(int spendingId, int userId)
        {
            Spending? spending = await _DbContext.Spendings
                .Include(spending => spending.BudgetCategory)
                .ThenInclude(category => category.Budget)
                .ThenInclude(budget => budget.Users)
                .Include(spending => spending.BudgetCategory)
                .ThenInclude(category => category.Budget)
                .ThenInclude(budget => budget.Currency)
                .Where(spending => spending.BudgetCategory.Budget.Users.Any(user => user.Id == userId) && spending.Id == spendingId && !spending.BudgetCategory.IsDeleted)
                .FirstOrDefaultAsync();

            return spending;
        }

        public int DeleteSpending(Spending spending)
        {
            _DbContext.Spendings.Remove(spending);
            return spending.Id;
        }

        public async Task<List<Spending>> GetSpendingsByCategoryAndPeriod(int budgetCategoryId, int budgetPeriodId, int userId)
        {
            bool hasAccess = await _DbContext.BudgetCategories
                .AnyAsync(bc => bc.Id == budgetCategoryId && bc.Budget.Users.Any(u => u.Id == userId));

            if (!hasAccess) return [];

            return await _DbContext.Spendings
                .Include(s => s.CreatedBy)
                .Include(s => s.BankTransaction)
                .Where(s => s.BudgetCategoryId == budgetCategoryId && s.BudgetPeriodId == budgetPeriodId)
                .OrderByDescending(s => s.Date)
                .ToListAsync();
        }
    }
}
