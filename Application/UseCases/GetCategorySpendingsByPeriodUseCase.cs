using Application.Contracts;
using Application.Dto.Budget;
using Application.Interfaces;
using Application.Mappers;
using Application.Services;
using Microsoft.Extensions.Logging;

namespace Application.UseCases
{
    public interface IGetCategorySpendingsByPeriodUseCase
    {
        Task<CaseResult<List<SpendingDto>>> InvokeAsync(int budgetCategoryId, int budgetPeriodId);
    }

    public class GetCategorySpendingsByPeriodUseCase : IGetCategorySpendingsByPeriodUseCase
    {
        private IUnitOfWork _UnitOfWork { get; set; }
        private IUserService _UserService { get; set; }
        private readonly ILogger<GetCategorySpendingsByPeriodUseCase> _Logger;

        public GetCategorySpendingsByPeriodUseCase(IUnitOfWork unitOfWork, IUserService userService, ILogger<GetCategorySpendingsByPeriodUseCase> logger)
        {
            _UnitOfWork = unitOfWork;
            _UserService = userService;
            _Logger = logger;
        }

        public async Task<CaseResult<List<SpendingDto>>> InvokeAsync(int budgetCategoryId, int budgetPeriodId)
        {
            var result = new CaseResult<List<SpendingDto>>([]);
            result.Successful = true;

            int userId = 0;
            try
            {
                userId = _UserService.GetUserId();
                var spendings = await _UnitOfWork.CategorySpendingsRepository.GetSpendingsByCategoryAndPeriod(budgetCategoryId, budgetPeriodId, userId);
                result.Data = spendings.Select(s => s.ToDto()).ToList();
                _Logger.LogInformation("Retrieved {Count} spendings for category {CategoryId} period {PeriodId} user {UserId}", result.Data.Count, budgetCategoryId, budgetPeriodId, userId);
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "Error retrieving spendings for category {CategoryId} period {PeriodId} user {UserId}", budgetCategoryId, budgetPeriodId, userId);
                result.Successful = false;
                result.ErrorMessage = "Something went wrong while retrieving spendings. Please try again later.";
            }

            return result;
        }
    }
}
