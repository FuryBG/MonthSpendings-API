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
            return await _DbContext.Budgets.Where(b => b.Id == budgetId && b.Users.Any(u => u.Id == userId)).Include(budget => budget.Users).Include(budget => budget.BudgetCategories).ThenInclude(bc => bc.Spendings).FirstOrDefaultAsync();

        }

        public async Task<List<Budget>> GetUserBudgets(int userId)
        {
            return await _DbContext.Budgets.Where(b => b.Users.Any(u => u.Id == userId)).Include(budget => budget.Users).Include(budget => budget.BudgetCategories).ThenInclude(bc => bc.Spendings).ToListAsync();
        }

        public Budget CreateBudget(Budget budget)
        {
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
            _DbContext.Budgets.Remove(budget);
            return budget;
        }

    }
}
