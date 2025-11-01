using Domain;

namespace Application.Interfaces.Repository
{
    public interface IBudgetRepository
    {
        Budget CreateBudget(Budget budget);
        Task<List<Budget>> GetUserBudgets(int userId);
        Budget UpdateBudget(Budget budget);
    }
}