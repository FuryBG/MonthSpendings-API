using Application.Interfaces.Repository;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository
{
    public class BudgetCategoryRepository : IBudgetCategoryRepository
    {
        private AppDbContext _DbContext { get; set; }
        public BudgetCategoryRepository(AppDbContext dbContext)
        {
            _DbContext = dbContext;
        }

        public async Task<BudgetCategory?> GetBudgetCategoryById(int budgetCategoryId, int userId)
        {
            return await _DbContext.BudgetCategories
                .Include(category => category.Spendings.Where(spending => spending.BudgetPeriod.EndDate == null))
                .Include(category => category.Budget)
                .ThenInclude(budget => budget.Users)
                .Where(bc => bc.Id == budgetCategoryId && bc.Budget.Users.Any(user => user.Id == userId))
                .FirstOrDefaultAsync();
        }

        public BudgetCategory CreateCategory(BudgetCategory budgetCategory)
        {
            _DbContext.BudgetCategories.Add(budgetCategory);
            return budgetCategory;
        }

        public BudgetCategory UpdateCategory(BudgetCategory budgetCategory)
        {
            _DbContext.BudgetCategories.Update(budgetCategory);
            return budgetCategory;
        }

        public BudgetCategory DeleteCategory(BudgetCategory budgetCategory)
        {
            _DbContext.BudgetCategories.Remove(budgetCategory);
            return budgetCategory;
        }
    }
}
