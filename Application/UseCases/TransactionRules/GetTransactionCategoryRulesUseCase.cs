using Application.Contracts;
using Application.Dto.Bank;
using Application.Interfaces;
using Application.Services;
using Domain.Bank;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.TransactionRules
{
    public interface IGetTransactionCategoryRulesUseCase
    {
        Task<CaseResult<List<TransactionCategoryRuleDto>>> InvokeAsync(CancellationToken cancellationToken);
    }

    public class GetTransactionCategoryRulesUseCase : IGetTransactionCategoryRulesUseCase
    {
        private readonly IUnitOfWork _UnitOfWork;
        private readonly IUserService _UserService;
        private readonly ILogger<GetTransactionCategoryRulesUseCase> _Logger;

        public GetTransactionCategoryRulesUseCase(IUnitOfWork unitOfWork, IUserService userService, ILogger<GetTransactionCategoryRulesUseCase> logger)
        {
            _UnitOfWork = unitOfWork;
            _UserService = userService;
            _Logger = logger;
        }

        public async Task<CaseResult<List<TransactionCategoryRuleDto>>> InvokeAsync(CancellationToken cancellationToken)
        {
            var result = new CaseResult<List<TransactionCategoryRuleDto>>();
            result.Successful = true;

            try
            {
                int userId = _UserService.GetUserId();
                List<TransactionCategoryRule> rules = await _UnitOfWork.TransactionCategoryRuleRepository.GetByUserIdAsync(userId, cancellationToken);
                result.Data = rules.Select(r => new TransactionCategoryRuleDto
                {
                    Id = r.Id,
                    Keyword = r.Keyword,
                    CategoryId = r.CategoryId,
                }).ToList();
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "Error getting transaction category rules");
                result.Successful = false;
                result.ErrorMessage = "Something went wrong while retrieving category rules. Please try again later.";
            }

            return result;
        }
    }
}
