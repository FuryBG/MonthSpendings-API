using Application.Contracts;
using Application.Interfaces;
using Application.Services;

namespace Application.UseCases.Savings
{
    public interface IRemoveSavingsContributionUseCase
    {
        Task<CaseResult<int>> InvokeAsync(int potId, int contributionId);
    }

    public class RemoveSavingsContributionUseCase : IRemoveSavingsContributionUseCase
    {
        private IUnitOfWork _UnitOfWork { get; set; }
        private IUserService _UserService { get; set; }
        public RemoveSavingsContributionUseCase(IUnitOfWork unitOfWork, IUserService userService)
        {
            _UnitOfWork = unitOfWork;
            _UserService = userService;
        }

        public async Task<CaseResult<int>> InvokeAsync(int potId, int contributionId)
        {
            var result = new CaseResult<int>();
            result.Successful = true;

            try
            {
                int userId = _UserService.GetUserId();
                var contribution = await _UnitOfWork.SavingsPotRepository.GetContributionById(contributionId);

                if (contribution == null || contribution.SavingsPotId != potId)
                {
                    result.Successful = false;
                    result.ErrorMessage = "Contribution not found.";
                    return result;
                }

                bool userHasAccess = contribution.SavingsPot.Users.Any(u => u.Id == userId);
                if (!userHasAccess)
                {
                    result.Successful = false;
                    result.ErrorMessage = "You don't have access to this savings pot.";
                    return result;
                }

                _UnitOfWork.SavingsPotRepository.RemoveContribution(contribution);
                await _UnitOfWork.CommitAsync();
                result.Data = contributionId;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                result.Successful = false;
                result.ErrorMessage = "Something went wrong while removing the contribution.";
            }

            return result;
        }
    }
}
