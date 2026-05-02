using Application.Contracts;
using Application.Dto.Bank;
using Application.Interfaces;
using Application.Mappers;
using Application.Services;
using Domain.Bank;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.Bank
{
    public interface IGetUncategorizedTransactionsUseCase
    {
        Task<CaseResult<List<BankTransactionDto>>> InvokeAsync(CancellationToken cancellationToken);
    }

    public class GetUncategorizedTransactionsUseCase : IGetUncategorizedTransactionsUseCase
    {
        private IUnitOfWork _UnitOfWork { get; set; }
        private IUserService _UserService { get; set; }
        private readonly ILogger<GetUncategorizedTransactionsUseCase> _Logger;

        public GetUncategorizedTransactionsUseCase(IUnitOfWork unitOfWork, IUserService userService, ILogger<GetUncategorizedTransactionsUseCase> logger)
        {
            _UnitOfWork = unitOfWork;
            _UserService = userService;
            _Logger = logger;
        }

        public async Task<CaseResult<List<BankTransactionDto>>> InvokeAsync(CancellationToken cancellationToken)
        {
            var result = new CaseResult<List<BankTransactionDto>>();
            result.Successful = true;

            int userId = 0;
            try
            {
                userId = _UserService.GetUserId();
                List<BankTransaction> transactions = await _UnitOfWork.BankTransactionRepository.GetUncategorizedTransactionsByUser(userId, cancellationToken);
                List<BankTransactionDto> transactionsDto = transactions.Select(transaction => transaction.ToDto()).ToList();
                result.Data = transactionsDto;
                _Logger.LogInformation("Retrieved {Count} uncategorized transactions for user {UserId}", result.Data!.Count, userId);
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "Error retrieving uncategorized transactions for user {UserId}", userId);
                result.Successful = false;
                result.ErrorMessage = "Something got wrong during getting banks. Please try again later.";
            }
            return result;
        }
    }
}
