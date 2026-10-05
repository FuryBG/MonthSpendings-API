using Application.Dto.Statistics;

namespace Application.Interfaces.Repository
{
    public interface IStatisticsRepository
    {
        Task<bool> HasBudgetAccess(int budgetId, int userId);
        /// <summary>All periods of the budget from the start of one boundary period to the other (order-insensitive), chronological, with their outgoing totals. Null if a boundary is not in the budget.</summary>
        Task<List<RangePeriodDto>?> GetPeriodTotals(int budgetId, int fromPeriodId, int toPeriodId, int? categoryId);
        /// <summary>Outgoing totals per category for the given periods, biggest first. Includes soft-deleted categories. Share is left at 0.</summary>
        Task<List<RangeCategoryDto>> GetCategoryTotals(IReadOnlyCollection<int> periodIds, int? categoryId);
        /// <summary>The biggest outgoing spendings of the given periods; amounts are positive.</summary>
        Task<List<RangeSpendingDto>> GetTopSpendings(IReadOnlyCollection<int> periodIds, int? categoryId, int top);
    }
}
