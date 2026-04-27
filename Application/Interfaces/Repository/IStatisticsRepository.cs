using Application.Dto.Statistics;
using Domain;

namespace Application.Interfaces.Repository
{
    public interface IStatisticsRepository
    {
        Task<PeriodComparisonDto?> GetPeriodComparison(int budgetId, int userId);
    }
}
