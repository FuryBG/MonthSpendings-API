using Application.Interfaces;
using Application.Interfaces.Repository;

namespace Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        private AppDbContext _DbContext { get; set; }
        public IUserRepository UserRepository { get; private set; }
        public IBudgetRepository BudgetRepository { get; private set; }
        public IBudgetCategoryRepository BudgetCategoryRepository { get; private set; }
        public ICategorySpendingsRepository CategorySpendingsRepository { get; private set; }
        public IBudgetInviteRepository BudgetInviteRepository { get; private set; }

        public UnitOfWork(AppDbContext dbContext, IUserRepository userRepository, IBudgetRepository budgetRepository, IBudgetCategoryRepository budgetCategoryRepository, ICategorySpendingsRepository categorySpendingsRepository, IBudgetInviteRepository budgetInviteRepository)
        {
            _DbContext = dbContext;
            UserRepository = userRepository;
            BudgetRepository = budgetRepository;
            BudgetCategoryRepository = budgetCategoryRepository;
            CategorySpendingsRepository = categorySpendingsRepository;
            BudgetInviteRepository = budgetInviteRepository;
        }

        public async Task CommitAsync()
        {
            await _DbContext.SaveChangesAsync();
        }
    }
}
