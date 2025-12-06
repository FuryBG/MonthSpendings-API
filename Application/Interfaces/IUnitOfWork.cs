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

        Task CommitAsync();
    }
}