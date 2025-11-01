using Domain;

namespace Application.Interfaces.Repository
{
    public interface ICategorySpendingsRepository
    {
        Spending AddSpending(Spending spending);
        Spending DeleteSpending(Spending spending);
    }
}