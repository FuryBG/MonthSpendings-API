using Application.Contracts;
using Application.Interfaces;
using Application.Services;

namespace Application.UseCases.Savings
{
    public interface IDeleteSavingsPotUseCase
    {
        Task<CaseResult<int>> InvokeAsync(int potId);
    }

    public class DeleteSavingsPotUseCase : IDeleteSavingsPotUseCase
    {
        private IUnitOfWork _UnitOfWork { get; set; }
        private IUserService _UserService { get; set; }
        public DeleteSavingsPotUseCase(IUnitOfWork unitOfWork, IUserService userService)
        {
            _UnitOfWork = unitOfWork;
            _UserService = userService;
        }

        public async Task<CaseResult<int>> InvokeAsync(int potId)
        {
            var result = new CaseResult<int>();
            result.Successful = true;

            try
            {
                int userId = _UserService.GetUserId();
                var pot = await _UnitOfWork.SavingsPotRepository.GetByIdForUser(potId, userId);

                if (pot == null)
                {
                    result.Successful = false;
                    result.ErrorMessage = "Savings pot not found.";
                    return result;
                }

                if (pot.CreatedByUserId != userId)
                {
                    result.Successful = false;
                    result.ErrorMessage = "Only the creator can delete this savings pot.";
                    return result;
                }

                _UnitOfWork.SavingsPotRepository.Delete(pot);
                await _UnitOfWork.CommitAsync();
                result.Data = potId;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                result.Successful = false;
                result.ErrorMessage = "Something went wrong while deleting the savings pot.";
            }

            return result;
        }
    }
}
