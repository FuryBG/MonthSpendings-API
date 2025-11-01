using Domain;

namespace Application.Interfaces.Repository
{
    public interface IBudgetCategoryRepository
    {
        BudgetCategory CreateCategory(BudgetCategory budgetCategory);
        BudgetCategory DeleteCategory(BudgetCategory budgetCategory);
        BudgetCategory UpdateCategory(BudgetCategory budgetCategory);
    }
}