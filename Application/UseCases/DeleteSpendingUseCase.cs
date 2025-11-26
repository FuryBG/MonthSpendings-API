using Application.Contracts;
using Application.Interfaces;
using Application.Services;
using Domain;

namespace Application.UseCases
{
    public interface IDeleteSpendingUseCase
    {
        Task<CaseResult<int?>> InvokeAsync(int spendingId);
    }

    public class DeleteSpendingUseCase : IDeleteSpendingUseCase
    {
        private IUnitOfWork _UnitOfWork { get; set; }
        private IUserService _UserService { get; set; }
        public DeleteSpendingUseCase(IUnitOfWork unitOfWork, IUserService userService)
        {
            _UnitOfWork = unitOfWork;
            _UserService = userService;
        }

        public async Task<CaseResult<int?>> InvokeAsync(int spendingId)
        {
            var result = new CaseResult<int?>();
            result.Successful = true;

            try
            {
                int userId = _UserService.GetUserId();
                Spending? spending = await _UnitOfWork.CategorySpendingsRepository.GetSpending(spendingId, userId);

                if (spending == null)
                {
                    result.Successful = false;
                    result.ErrorMessage = $"Can't find spending with id {spendingId} to delete. Please try again later.";
                    return result;
                }

                _UnitOfWork.CategorySpendingsRepository.DeleteSpending(spending);
                await _UnitOfWork.CommitAsync();
                result.Data = spendingId;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                result.Successful = false;
                result.ErrorMessage = $"Something got wrong during deleting spending with id {spendingId}. Please try again later.";
            }

            return result;
        }
    }
}
