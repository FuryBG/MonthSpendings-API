using Application.Contracts;
using Application.Dto.Bank;
using Application.Interfaces;
using Application.Mappers;
using Application.Services;
using Domain.Bank;

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

        public GetUncategorizedTransactionsUseCase(IUnitOfWork unitOfWork, IUserService userService)
        {
            _UnitOfWork = unitOfWork;
            _UserService = userService;
        }

        public async Task<CaseResult<List<BankTransactionDto>>> InvokeAsync(CancellationToken cancellationToken)
        {
            var result = new CaseResult<List<BankTransactionDto>>();
            result.Successful = true;

            try
            {
                int userId = _UserService.GetUserId();
                List<BankTransaction> transactions = await _UnitOfWork.BankTransactionRepository.GetUncategorizedTransactionsByUser(userId, cancellationToken);
                List<BankTransactionDto> transactionsDto = transactions.Select(transaction => transaction.ToDto()).ToList();
                result.Data = transactionsDto;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                result.Successful = false;
                result.ErrorMessage = "Something got wrong during getting banks. Please try again later.";
            }
            return result;
        }
    }
}