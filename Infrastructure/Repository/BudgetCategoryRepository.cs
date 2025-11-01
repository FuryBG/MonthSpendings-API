using Application.Interfaces.Repository;
using Domain;

namespace Infrastructure.Repository
{
    public class BudgetCategoryRepository : IBudgetCategoryRepository
    {
        private AppDbContext _DbContext { get; set; }
        public BudgetCategoryRepository(AppDbContext dbContext)
        {
            _DbContext = dbContext;
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
