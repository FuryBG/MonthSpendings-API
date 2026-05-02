using Application.Contracts;
using Application.Dto.Statistics;
using Application.Interfaces;
using Application.Services;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.Statistics
{
    public interface IGetPeriodComparisonUseCase
    {
        Task<CaseResult<PeriodComparisonDto?>> InvokeAsync(int budgetId);
    }

    public class GetPeriodComparisonUseCase : IGetPeriodComparisonUseCase
    {
        private IUnitOfWork _UnitOfWork { get; set; }
        private IUserService _UserService { get; set; }
        private readonly ILogger<GetPeriodComparisonUseCase> _Logger;
        public GetPeriodComparisonUseCase(IUnitOfWork unitOfWork, IUserService userService, ILogger<GetPeriodComparisonUseCase> logger)
        {
            _UnitOfWork = unitOfWork;
            _UserService = userService;
            _Logger = logger;
        }

        public async Task<CaseResult<PeriodComparisonDto?>> InvokeAsync(int budgetId)
        {
            var result = new CaseResult<PeriodComparisonDto?>();
            result.Successful = true;
            int userId = 0;

            try
            {
                userId = _UserService.GetUserId();
                var comparison = await _UnitOfWork.StatisticsRepository.GetPeriodComparison(budgetId, userId);

                if (comparison == null)
                {
                    result.Successful = false;
                    result.ErrorMessage = "Budget not found or you don't have access.";
                    return result;
                }

                result.Data = comparison;
                _Logger.LogInformation("Period comparison retrieved for budget {BudgetId} by user {UserId}", budgetId, userId);
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "Error getting period comparison for user {UserId}", userId);
                result.Successful = false;
                result.ErrorMessage = "Something went wrong while fetching period comparison.";
            }

            return result;
        }
    }
}
