using Application.Contracts;
using Application.Dto.Savings;
using Application.Interfaces;
using Application.Mappers;
using Application.Services;

namespace Application.UseCases.Savings
{
    public interface IGetSavingsHistoryUseCase
    {
        Task<CaseResult<SavingsHistoryDto?>> InvokeAsync(int potId);
    }

    public class GetSavingsHistoryUseCase : IGetSavingsHistoryUseCase
    {
        private IUnitOfWork _UnitOfWork { get; set; }
        private IUserService _UserService { get; set; }
        public GetSavingsHistoryUseCase(IUnitOfWork unitOfWork, IUserService userService)
        {
            _UnitOfWork = unitOfWork;
            _UserService = userService;
        }

        public async Task<CaseResult<SavingsHistoryDto?>> InvokeAsync(int potId)
        {
            var result = new CaseResult<SavingsHistoryDto?>();
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

                var allContributions = await _UnitOfWork.SavingsPotRepository.GetContributions(potId);

                var months = allContributions
                    .GroupBy(c => new { c.Date.Year, c.Date.Month })
                    .OrderByDescending(g => g.Key.Year).ThenByDescending(g => g.Key.Month)
                    .Select(g => new MonthlyContributionDto
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        Total = g.Sum(c => c.Amount),
                        Contributions = g.Select(c => c.ToDto()).ToList(),
                    })
                    .ToList();

                result.Data = new SavingsHistoryDto
                {
                    RunningTotal = allContributions.Sum(c => c.Amount),
                    Months = months,
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                result.Successful = false;
                result.ErrorMessage = "Something went wrong while fetching savings history.";
            }

            return result;
        }
    }
}
