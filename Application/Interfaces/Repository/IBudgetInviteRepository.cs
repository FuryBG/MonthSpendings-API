using Domain;

namespace Application.Interfaces.Repository
{
    public interface IBudgetInviteRepository
    {
        BudgetInvite CreateInvite(BudgetInvite budgetInvite);
        BudgetInvite DeleteInvite(BudgetInvite budgetInvite);
        Task<List<BudgetInvite>> GetBudgetInvites(int userId);
        BudgetInvite UpdateInvite(BudgetInvite budgetInvite);
    }
}