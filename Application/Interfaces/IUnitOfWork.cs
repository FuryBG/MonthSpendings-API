using Application.Interfaces.Repository;
using Application.Interfaces.Repository.Bank;

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
        IBankConsentRepository BankConsentRepository { get; }
        IBankAccountRepository BankAccountRepository { get; }
        IBankTransactionRepository BankTransactionRepository { get; }
        ISavingsPotRepository SavingsPotRepository { get; }
        ISavingsPotInviteRepository SavingsPotInviteRepository { get; }
        IStatisticsRepository StatisticsRepository { get; }

        Task CommitAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}