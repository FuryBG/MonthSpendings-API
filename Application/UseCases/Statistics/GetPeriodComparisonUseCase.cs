using Application.Contracts;
using Application.Dto.Statistics;
using Application.Interfaces;
using Application.Services;

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
        public GetPeriodComparisonUseCase(IUnitOfWork unitOfWork, IUserService userService)
        {
            _UnitOfWork = unitOfWork;
            _UserService = userService;
        }

        public async Task<CaseResult<PeriodComparisonDto?>> InvokeAsync(int budgetId)
        {
            var result = new CaseResult<PeriodComparisonDto?>();
            result.Successful = true;

            try
            {
                int userId = _UserService.GetUserId();
                var comparison = await _UnitOfWork.StatisticsRepository.GetPeriodComparison(budgetId, userId);

                if (comparison == null)
                {
                    result.Successful = false;
                    result.ErrorMessage = "Budget not found or you don't have access.";
                    return result;
                }

                result.Data = comparison;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                result.Successful = false;
                result.ErrorMessage = "Something went wrong while fetching period comparison.";
            }

            return result;
        }
    }
}
