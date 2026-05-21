using Application.Contracts;
using Application.Dto.Bank;
using Application.Interfaces;
using Application.Services;
using Domain.Bank;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.TransactionRules
{
    public interface ICreateTransactionCategoryRuleUseCase
    {
        Task<CaseResult<TransactionCategoryRuleDto>> InvokeAsync(CreateTransactionCategoryRuleDto dto, CancellationToken cancellationToken);
    }

    public class CreateTransactionCategoryRuleUseCase : ICreateTransactionCategoryRuleUseCase
    {
        private readonly IUnitOfWork _UnitOfWork;
        private readonly IUserService _UserService;
        private readonly ILogger<CreateTransactionCategoryRuleUseCase> _Logger;

        public CreateTransactionCategoryRuleUseCase(IUnitOfWork unitOfWork, IUserService userService, ILogger<CreateTransactionCategoryRuleUseCase> logger)
        {
            _UnitOfWork = unitOfWork;
            _UserService = userService;
            _Logger = logger;
        }

        public async Task<CaseResult<TransactionCategoryRuleDto>> InvokeAsync(CreateTransactionCategoryRuleDto dto, CancellationToken cancellationToken)
        {
            var result = new CaseResult<TransactionCategoryRuleDto>();
            result.Successful = true;

            try
            {
                int userId = _UserService.GetUserId();

                Domain.BudgetCategory? category = await _UnitOfWork.BudgetCategoryRepository.GetBudgetCategoryById(dto.CategoryId, userId);
                if (category == null)
                {
                    result.Successful = false;
                    result.ErrorMessage = "Category not found.";
                    return result;
                }

                TransactionCategoryRule rule = new TransactionCategoryRule
                {
                    UserId = userId,
                    Keyword = dto.Keyword.Trim(),
                    CategoryId = dto.CategoryId,
                };

                await _UnitOfWork.TransactionCategoryRuleRepository.AddAsync(rule, cancellationToken);
                await _UnitOfWork.CommitAsync();

                result.Data = new TransactionCategoryRuleDto
                {
                    Id = rule.Id,
                    Keyword = rule.Keyword,
                    CategoryId = rule.CategoryId,
                };
                _Logger.LogInformation("Created transaction category rule {RuleId} for user {UserId}", rule.Id, userId);
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "Error creating transaction category rule");
                result.Successful = false;
                result.ErrorMessage = "Something went wrong while creating the category rule. Please try again later.";
            }

            return result;
        }
    }
}
