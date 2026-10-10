using Application.Resources;
using Application.Contracts;
using Application.Dto.Statistics;
using Application.Interfaces;
using Application.Services;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.Statistics
{
    public interface IGetRangeSummaryUseCase
    {
        Task<CaseResult<RangeSummaryDto>> InvokeAsync(int budgetId, int fromPeriodId, int toPeriodId, int? categoryId, int top);
    }

    public class GetRangeSummaryUseCase : IGetRangeSummaryUseCase
    {
        public const int DefaultTop = 5;
        public const int MaxTop = 50;

        private IUnitOfWork _UnitOfWork { get; set; }
        private IUserService _UserService { get; set; }
        private readonly ILogger<GetRangeSummaryUseCase> _Logger;

        public GetRangeSummaryUseCase(IUnitOfWork unitOfWork, IUserService userService, ILogger<GetRangeSummaryUseCase> logger)
        {
            _UnitOfWork = unitOfWork;
            _UserService = userService;
            _Logger = logger;
        }

        public async Task<CaseResult<RangeSummaryDto>> InvokeAsync(int budgetId, int fromPeriodId, int toPeriodId, int? categoryId, int top)
        {
            var result = new CaseResult<RangeSummaryDto>();
            result.Successful = true;
            int userId = _UserService.GetUserId();
            var repository = _UnitOfWork.StatisticsRepository;

            if (!await repository.HasBudgetAccess(budgetId, userId))
            {
                _Logger.LogWarning("User {UserId} requested statistics for budget {BudgetId} without access", userId, budgetId);
                return CaseResult<RangeSummaryDto>.Error(Messages.StatisticsBudgetNotAccessible);
            }

            var periods = await repository.GetPeriodTotals(budgetId, fromPeriodId, toPeriodId, categoryId);
            if (periods == null || periods.Count == 0)
            {
                _Logger.LogInformation("No periods {FromPeriodId}..{ToPeriodId} in budget {BudgetId} for statistics (user {UserId})", fromPeriodId, toPeriodId, budgetId, userId);
                return CaseResult<RangeSummaryDto>.Error(Messages.StatisticsPeriodsMismatch);
            }

            var periodIds = periods.Select(p => p.PeriodId).ToList();
            var categories = await repository.GetCategoryTotals(periodIds, categoryId);
            var topSpendings = await repository.GetTopSpendings(periodIds, categoryId, Math.Clamp(top, 1, MaxTop));

            decimal total = periods.Sum(p => p.Total);

            foreach (var category in categories)
            {
                category.Share = total == 0 ? 0 : Math.Round(category.Amount / total * 100, 1);
            }

            result.Data = new RangeSummaryDto
            {
                StartDate = periods[0].StartDate,
                EndDate = periods[^1].EndDate,
                Total = total,
                AveragePerPeriod = Math.Round(total / periods.Count, 2),
                Periods = periods,
                Categories = categories,
                TopSpendings = topSpendings,
            };
            _Logger.LogDebug("Range summary retrieved for budget {BudgetId} ({Count} periods) by user {UserId}", budgetId, periods.Count, userId);

            return result;
        }
    }
}
