using Domain;

namespace Application.Interfaces.Repository
{
    public interface ICategorySpendingsRepository
    {
        public Task<Spending?> GetSpending(int spendingId, int userId);
        Spending AddSpending(Spending spending);
        public int DeleteSpending(Spending spending);
    }
}