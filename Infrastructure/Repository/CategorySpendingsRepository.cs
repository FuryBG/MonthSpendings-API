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
                .Where(spending => spending.BudgetCategory.Budget.Users.Any(user => user.Id == userId) && spending.Id == spendingId)
                .FirstOrDefaultAsync();

            return spending;
        }

        public int DeleteSpending(Spending spending)
        {
            _DbContext.Spendings.Remove(spending);
            return spending.Id;
        }
    }
}
