using Domain;

namespace Application.Interfaces.Repository
{
    public interface ISavingsPotInviteRepository
    {
        Task<SavingsPotInvite?> GetById(int inviteId);
        SavingsPotInvite Create(SavingsPotInvite invite);
        SavingsPotInvite Update(SavingsPotInvite invite);
    }
}
