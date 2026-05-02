using Application.Contracts;
using Application.Interfaces;
using Application.Services;
using Microsoft.Extensions.Logging;

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
        private readonly ILogger<RemoveSavingsContributionUseCase> _Logger;
        public RemoveSavingsContributionUseCase(IUnitOfWork unitOfWork, IUserService userService, ILogger<RemoveSavingsContributionUseCase> logger)
        {
            _UnitOfWork = unitOfWork;
            _UserService = userService;
            _Logger = logger;
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
                _Logger.LogInformation("Contribution {ContributionId} removed from pot {PotId} by user {UserId}", contributionId, potId, userId);
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "Error removing contribution {ContributionId}", contributionId);
                result.Successful = false;
                result.ErrorMessage = "Something went wrong while removing the contribution.";
            }

            return result;
        }
    }
}
