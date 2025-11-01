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

        public async Task<List<Budget>> GetUserBudgets(int userId)
        {
            return await _DbContext.Budgets.Where(b => b.Users.Any(u => u.Id == userId)).ToListAsync();
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

    }
}
