using Application.Interfaces.Repository;
using Domain;

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

        public Spending DeleteSpending(Spending spending)
        {
            _DbContext.Spendings.Remove(spending);
            return spending;
        }
    }
}
