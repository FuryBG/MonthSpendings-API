using Application.Contracts;
using Application.Dto.Statistics;
using Application.Interfaces;
using Application.Services;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.Statistics
{
    public interface IGetPeriodsHistoryUseCase
    {
        Task<CaseResult<List<PeriodHistoryItemDto>>> InvokeAsync(int budgetId);
    }

    public class GetPeriodsHistoryUseCase : IGetPeriodsHistoryUseCase
    {
        private IUnitOfWork _UnitOfWork { get; set; }
        private IUserService _UserService { get; set; }
        private readonly ILogger<GetPeriodsHistoryUseCase> _Logger;

        public GetPeriodsHistoryUseCase(IUnitOfWork unitOfWork, IUserService userService, ILogger<GetPeriodsHistoryUseCase> logger)
        {
            _UnitOfWork = unitOfWork;
            _UserService = userService;
            _Logger = logger;
        }

        public async Task<CaseResult<List<PeriodHistoryItemDto>>> InvokeAsync(int budgetId)
        {
            var result = new CaseResult<List<PeriodHistoryItemDto>>();
            result.Successful = true;
            int userId = 0;

            try
            {
                userId = _UserService.GetUserId();
                var history = await _UnitOfWork.StatisticsRepository.GetPeriodsHistory(budgetId, userId);
                result.Data = history;
                _Logger.LogInformation("Periods history retrieved for budget {BudgetId} by user {UserId}", budgetId, userId);
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "Error getting periods history for user {UserId}", userId);
                result.Successful = false;
                result.ErrorMessage = "Something went wrong while fetching periods history.";
            }

            return result;
        }
    }
}
