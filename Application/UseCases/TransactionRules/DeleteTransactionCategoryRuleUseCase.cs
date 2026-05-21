using Application.Contracts;
using Application.Interfaces;
using Application.Services;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.TransactionRules
{
    public interface IDeleteTransactionCategoryRuleUseCase
    {
        Task<CaseResult<int>> InvokeAsync(int ruleId, CancellationToken cancellationToken);
    }

    public class DeleteTransactionCategoryRuleUseCase : IDeleteTransactionCategoryRuleUseCase
    {
        private readonly IUnitOfWork _UnitOfWork;
        private readonly IUserService _UserService;
        private readonly ILogger<DeleteTransactionCategoryRuleUseCase> _Logger;

        public DeleteTransactionCategoryRuleUseCase(IUnitOfWork unitOfWork, IUserService userService, ILogger<DeleteTransactionCategoryRuleUseCase> logger)
        {
            _UnitOfWork = unitOfWork;
            _UserService = userService;
            _Logger = logger;
        }

        public async Task<CaseResult<int>> InvokeAsync(int ruleId, CancellationToken cancellationToken)
        {
            var result = new CaseResult<int>();
            result.Successful = true;

            try
            {
                int userId = _UserService.GetUserId();
                int deleted = await _UnitOfWork.TransactionCategoryRuleRepository.DeleteByIdAsync(ruleId, userId, cancellationToken);

                if (deleted == 0)
                {
                    result.Successful = false;
                    result.ErrorMessage = "Rule not found.";
                    return result;
                }

                result.Data = ruleId;
                _Logger.LogInformation("Deleted transaction category rule {RuleId} for user {UserId}", ruleId, userId);
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "Error deleting transaction category rule {RuleId}", ruleId);
                result.Successful = false;
                result.ErrorMessage = "Something went wrong while deleting the category rule. Please try again later.";
            }

            return result;
        }
    }
}
