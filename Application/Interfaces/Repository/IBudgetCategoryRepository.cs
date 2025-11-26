using Domain;

namespace Application.Interfaces.Repository
{
    public interface IBudgetCategoryRepository
    {
        Task<BudgetCategory?> GetBudgetCategoryById(int budgetCategoryId, int userId);
        BudgetCategory CreateCategory(BudgetCategory budgetCategory);
        BudgetCategory DeleteCategory(BudgetCategory budgetCategory);
        BudgetCategory UpdateCategory(BudgetCategory budgetCategory);
    }
}