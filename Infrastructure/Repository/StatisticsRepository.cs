using Application.Dto.Statistics;
using Application.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository
{
    public class StatisticsRepository : IStatisticsRepository
    {
        private AppDbContext _DbContext { get; set; }
        public StatisticsRepository(AppDbContext dbContext)
        {
            _DbContext = dbContext;
        }

        public async Task<PeriodComparisonDto?> GetPeriodComparison(int budgetId, int userId)
        {
            bool userHasAccess = await _DbContext.Budgets
                .AnyAsync(b => b.Id == budgetId && b.Users.Any(u => u.Id == userId));

            if (!userHasAccess) return null;

            var periods = await _DbContext.BudgetPeriods
                .Where(p => p.BudgetId == budgetId)
                .OrderByDescending(p => p.StartDate)
                .Take(2)
                .ToListAsync();

            if (periods.Count == 0) return null;

            var categories = await _DbContext.BudgetCategories
                .IgnoreQueryFilters()
                .Where(c => c.BudgetId == budgetId)
                .Select(c => new { c.Id, c.Name, c.IsDeleted })
                .ToListAsync();

            var currentPeriod = periods[0];
            var previousPeriod = periods.Count > 1 ? periods[1] : null;

            var currentSpendingMap = await BuildSpendingMap(currentPeriod.Id);

            var currentCategoryDtos = categories.Select(c => new CategoryComparisonDto
            {
                CategoryId = c.Id,
                CategoryName = c.Name,
                Amount = currentSpendingMap.GetValueOrDefault(c.Id, 0),
                IsDeleted = c.IsDeleted,
            }).ToList();

            decimal currentTotal = currentCategoryDtos.Sum(c => c.Amount);

            PeriodSummaryDto? previousPeriodDto = null;
            decimal previousTotal = 0;

            if (previousPeriod != null)
            {
                var previousSpendingMap = await BuildSpendingMap(previousPeriod.Id);

                var previousCategoryDtos = categories.Select(c => new CategoryComparisonDto
                {
                    CategoryId = c.Id,
                    CategoryName = c.Name,
                    Amount = previousSpendingMap.GetValueOrDefault(c.Id, 0),
                    IsDeleted = c.IsDeleted,
                }).ToList();

                previousTotal = previousCategoryDtos.Sum(c => c.Amount);

                previousPeriodDto = new PeriodSummaryDto
                {
                    PeriodId = previousPeriod.Id,
                    StartDate = previousPeriod.StartDate,
                    EndDate = previousPeriod.EndDate,
                    TotalSpent = previousTotal,
                    Categories = previousCategoryDtos,
                };
            }

            decimal delta = currentTotal - previousTotal;
            decimal? deltaPercent = previousTotal != 0
                ? Math.Round(delta / previousTotal * 100, 1)
                : null;

            return new PeriodComparisonDto
            {
                CurrentPeriod = new PeriodSummaryDto
                {
                    PeriodId = currentPeriod.Id,
                    StartDate = currentPeriod.StartDate,
                    EndDate = currentPeriod.EndDate,
                    TotalSpent = currentTotal,
                    Categories = currentCategoryDtos,
                },
                PreviousPeriod = previousPeriodDto,
                TotalDelta = delta,
                TotalDeltaPercent = deltaPercent,
            };
        }

        private async Task<Dictionary<int, decimal>> BuildSpendingMap(int periodId)
        {
            return await _DbContext.Spendings
                .Where(s => s.BudgetPeriodId == periodId)
                .GroupBy(s => s.BudgetCategoryId)
                .Select(g => new { CategoryId = g.Key, Total = g.Sum(s => s.Amount) })
                .ToDictionaryAsync(x => x.CategoryId, x => x.Total);
        }
    }
}
