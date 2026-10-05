using Application.Dto.Statistics;
using Application.Interfaces.Repository;
using Domain;
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

        public async Task<bool> HasBudgetAccess(int budgetId, int userId)
        {
            return await _DbContext.Budgets
                .AnyAsync(b => b.Id == budgetId && b.Users.Any(u => u.Id == userId));
        }

        public async Task<List<RangePeriodDto>?> GetPeriodTotals(int budgetId, int fromPeriodId, int toPeriodId, int? categoryId)
        {
            var bounds = await _DbContext.BudgetPeriods
                .Where(p => p.BudgetId == budgetId && (p.Id == fromPeriodId || p.Id == toPeriodId))
                .Select(p => p.StartDate)
                .ToListAsync();

            bool bothFound = fromPeriodId == toPeriodId ? bounds.Count == 1 : bounds.Count == 2;
            if (!bothFound) return null;

            var rangeStart = bounds.Min();
            var rangeEnd = bounds.Max();

            return await _DbContext.BudgetPeriods
                .Where(p => p.BudgetId == budgetId && p.StartDate >= rangeStart && p.StartDate <= rangeEnd)
                .OrderBy(p => p.StartDate)
                .Select(p => new RangePeriodDto
                {
                    PeriodId = p.Id,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    Total = _DbContext.Spendings
                        .Where(s => s.BudgetPeriodId == p.Id && s.Amount < 0
                            && (categoryId == null || s.BudgetCategoryId == categoryId))
                        .Sum(s => (decimal?)(-s.Amount)) ?? 0,
                })
                .ToListAsync();
        }

        public async Task<List<RangeCategoryDto>> GetCategoryTotals(IReadOnlyCollection<int> periodIds, int? categoryId)
        {
            return await OutgoingSpendings(periodIds, categoryId)
                .GroupBy(s => new { s.BudgetCategoryId, s.BudgetCategory.Name, s.BudgetCategory.IsDeleted })
                .Select(g => new RangeCategoryDto
                {
                    CategoryId = g.Key.BudgetCategoryId,
                    Name = g.Key.Name,
                    IsDeleted = g.Key.IsDeleted,
                    Amount = g.Sum(s => -s.Amount),
                })
                .OrderByDescending(c => c.Amount)
                .ToListAsync();
        }

        public async Task<List<RangeSpendingDto>> GetTopSpendings(IReadOnlyCollection<int> periodIds, int? categoryId, int top)
        {
            return await OutgoingSpendings(periodIds, categoryId)
                .OrderBy(s => s.Amount) // most negative first = biggest spending
                .ThenByDescending(s => s.Date)
                .Take(top)
                .Select(s => new RangeSpendingDto
                {
                    Id = s.Id,
                    Amount = -s.Amount,
                    Description = s.Description,
                    CategoryId = s.BudgetCategoryId,
                    CategoryName = s.BudgetCategory.Name,
                    Date = s.Date,
                })
                .ToListAsync();
        }

        // Ignores query filters so spendings of soft-deleted categories still count.
        private IQueryable<Spending> OutgoingSpendings(IReadOnlyCollection<int> periodIds, int? categoryId)
        {
            var query = _DbContext.Spendings
                .IgnoreQueryFilters()
                .Where(s => periodIds.Contains(s.BudgetPeriodId) && s.Amount < 0);

            if (categoryId.HasValue)
                query = query.Where(s => s.BudgetCategoryId == categoryId.Value);

            return query;
        }
    }
}
