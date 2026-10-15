using Domain;

namespace Application.Interfaces.Repository
{
    public interface IBudgetRepository
    {
        Task<Budget?> GetBudgetById(int budgetId, int userId);
        Task<List<Budget>> GetUserBudgets(int userId);
        /// <summary>Currency code of the user's own oldest budget (else any shared one); null when the user has no budget.</summary>
        Task<string?> GetPrimaryCurrencyCode(int userId);
        Budget CreateBudget(Budget budget);
        Budget UpdateBudget(Budget budget);
        Budget DeleteBudget(Budget budget);
    }
}