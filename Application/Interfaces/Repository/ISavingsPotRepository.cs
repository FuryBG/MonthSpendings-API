using Domain;

namespace Application.Interfaces.Repository
{
    public interface ISavingsPotRepository
    {
        Task<SavingsPot?> GetByIdForUser(int savingsPotId, int userId);
        Task<List<SavingsPot>> GetAllForUser(int userId);
        SavingsPot Create(SavingsPot savingsPot);
        SavingsPot Delete(SavingsPot savingsPot);
        Task<SavingsContribution?> GetContributionById(int contributionId);
        SavingsContribution AddContribution(SavingsContribution contribution);
        SavingsContribution RemoveContribution(SavingsContribution contribution);
        Task<List<SavingsContribution>> GetContributions(int savingsPotId);
    }
}
