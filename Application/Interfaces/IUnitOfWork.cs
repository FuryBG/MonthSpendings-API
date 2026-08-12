using Application.Interfaces.Repository;

namespace Application.Interfaces
{
    public interface IUnitOfWork
    {
        IBudgetCategoryRepository BudgetCategoryRepository { get; }
        IBudgetRepository BudgetRepository { get; }
        ICategorySpendingsRepository CategorySpendingsRepository { get; }
        IBudgetInviteRepository BudgetInviteRepository { get; }
        IUserRepository UserRepository { get; }
        ICurrencyRepository CurrencyRepository { get; }
        INotificationTransactionRepository NotificationTransactionRepository { get; }
        ITransactionCategoryRuleRepository TransactionCategoryRuleRepository { get; }
        IStatisticsRepository StatisticsRepository { get; }
        IAccountDeleteRequestRepository AccountDeleteRequestRepository { get; }

        Task CommitAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
