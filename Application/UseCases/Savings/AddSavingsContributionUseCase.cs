using Application.Contracts;
using Application.Dto.Savings;
using Application.Interfaces;
using Application.Mappers;
using Application.Services;
using Domain;

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
        public AddSavingsContributionUseCase(IUnitOfWork unitOfWork, IUserService userService)
        {
            _UnitOfWork = unitOfWork;
            _UserService = userService;
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
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                result.Successful = false;
                result.ErrorMessage = "Something went wrong while adding the contribution.";
            }

            return result;
        }
    }
}
