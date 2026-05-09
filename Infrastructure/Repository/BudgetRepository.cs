using Application.Interfaces.Repository;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository
{
    public class BudgetRepository : IBudgetRepository
    {
        private AppDbContext _DbContext { get; set; }
        public BudgetRepository(AppDbContext dbContext)
        {
            _DbContext = dbContext;
        }

        public async Task<Budget?> GetBudgetById(int budgetId, int userId)
        {
            Budget? budget = await _DbContext.Budgets
                .Include(budget => budget.Currency)
                .Include(budget => budget.Users)
                .Include(budget => budget.BudgetPeriods.Where(budgetPeriod => budgetPeriod.EndDate == null))
                .Include(budget => budget.BudgetCategories)
                .ThenInclude(budgetCategory => budgetCategory.Spendings.Where(spending => spending.BudgetPeriod.EndDate == null))
                .Where(b => b.Id == budgetId && b.Users.Any(u => u.Id == userId))
                .FirstOrDefaultAsync();

            if (budget == null)
            {
                return budget;
            }

            BudgetPeriod activePeriod = budget.BudgetPeriods.First(budgetPeriod => budgetPeriod.EndDate == null);

            foreach (BudgetCategory budgetCategory in budget.BudgetCategories)
            {
                var spendings = await _DbContext.Spendings
                    .Include(s => s.CreatedBy)
                    .Where(s => s.BudgetCategoryId == budgetCategory.Id && s.Date >= activePeriod.StartDate)
                    .ToListAsync();

                budgetCategory.Spendings = spendings;
            }

            return budget;
        }

        public async Task<List<Budget>> GetUserBudgets(int userId)
        {
            return await _DbContext.Budgets
                .Include(budget => budget.Currency)
                .Include(budget => budget.Users)
                .Include(budget => budget.BudgetPeriods.Where(budgetPeriod => budgetPeriod.EndDate == null))
                .Include(budget => budget.BudgetCategories)
                .ThenInclude(budgetCategory => budgetCategory.Spendings.Where(spending => spending.BudgetPeriod.EndDate == null))
                .ThenInclude(spending => spending.CreatedBy)
                .Where(b => b.Users.Any(u => u.Id == userId))
                .ToListAsync();
        }

        public Budget CreateBudget(Budget budget)
        {
            _DbContext.Attach(budget.Currency);
            _DbContext.Budgets.Add(budget);
            return budget;
        }

        public Budget UpdateBudget(Budget budget)
        {
            _DbContext.Budgets.Update(budget);
            return budget;
        }

        public Budget DeleteBudget(Budget budget)
        {
            budget.IsDeleted = true;
            foreach (var category in budget.BudgetCategories)
            {
                category.IsDeleted = true;
            }
            _DbContext.Budgets.Update(budget);
            return budget;
        }

    }
}
