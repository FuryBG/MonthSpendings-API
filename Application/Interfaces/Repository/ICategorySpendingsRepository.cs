using Domain;

namespace Application.Interfaces.Repository
{
    public interface ICategorySpendingsRepository
    {
        public Task<Spending?> GetSpending(int spendingId, int userId);
        Spending AddSpending(Spending spending);
        public int DeleteSpending(Spending spending);
        public Task<List<Spending>> GetSpendingsByCategoryAndPeriod(int budgetCategoryId, int budgetPeriodId, int userId);
    }
}