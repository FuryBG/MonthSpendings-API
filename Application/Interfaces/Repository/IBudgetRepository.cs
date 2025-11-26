using Domain;

namespace Application.Interfaces.Repository
{
    public interface IBudgetRepository
    {
        Task<Budget?> GetBudgetById(int budgetId, int userId);
        Task<List<Budget>> GetUserBudgets(int userId);
        Budget CreateBudget(Budget budget);
        Budget UpdateBudget(Budget budget);
        Budget DeleteBudget(Budget budget);
    }
}