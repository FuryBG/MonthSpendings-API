using Application.Contracts;
using Application.Dto.Savings;
using Application.Interfaces;
using Application.Mappers;
using Application.Services;
using Domain;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.Savings
{
    public interface IAddSavingsContributionUseCase
    {
        Task<CaseResult<SavingsContributionDto?>> InvokeAsync(int potId, SavingsContributionDto dto);
    }

    public class AddSavingsContributionUseCase : IAddSavingsContributionUseCase
    {
        private IUnitOfWork _UnitOfWork { get; set; }
        private IUserService _UserService { get; set; }
        private readonly ILogger<AddSavingsContributionUseCase> _Logger;
        public AddSavingsContributionUseCase(IUnitOfWork unitOfWork, IUserService userService, ILogger<AddSavingsContributionUseCase> logger)
        {
            _UnitOfWork = unitOfWork;
            _UserService = userService;
            _Logger = logger;
        }

        public async Task<CaseResult<SavingsContributionDto?>> InvokeAsync(int potId, SavingsContributionDto dto)
        {
            var result = new CaseResult<SavingsContributionDto?>();
            result.Successful = true;

            try
            {
                int userId = _UserService.GetUserId();
                var pot = await _UnitOfWork.SavingsPotRepository.GetByIdForUser(potId, userId);

                if (pot == null)
                {
                    result.Successful = false;
                    result.ErrorMessage = "Savings pot not found.";
                    return result;
                }

                if (dto.Amount <= 0)
                {
                    result.Successful = false;
                    result.ErrorMessage = "Contribution amount must be greater than zero.";
                    return result;
                }

                var contribution = new SavingsContribution
                {
                    SavingsPotId = potId,
                    Amount = dto.Amount,
                    Date = DateTime.UtcNow,
                    Note = dto.Note,
                    AddedByUserId = userId,
                };

                var created = _UnitOfWork.SavingsPotRepository.AddContribution(contribution);
                await _UnitOfWork.CommitAsync();

                // Re-load with AddedBy populated for the response
                var loaded = await _UnitOfWork.SavingsPotRepository.GetContributionById(created.Id);
                result.Data = loaded?.ToDto() ?? created.ToDto();
                _Logger.LogInformation("Contribution {ContributionId} added to pot {PotId} by user {UserId}", created.Id, potId, userId);
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "Error adding contribution to pot {PotId}", potId);
                result.Successful = false;
                result.ErrorMessage = "Something went wrong while adding the contribution.";
            }

            return result;
        }
    }
}
