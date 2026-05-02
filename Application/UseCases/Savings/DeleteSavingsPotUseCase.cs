using Application.Contracts;
using Application.Interfaces;
using Application.Services;
using Microsoft.Extensions.Logging;

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
        private readonly ILogger<DeleteSavingsPotUseCase> _Logger;
        public DeleteSavingsPotUseCase(IUnitOfWork unitOfWork, IUserService userService, ILogger<DeleteSavingsPotUseCase> logger)
        {
            _UnitOfWork = unitOfWork;
            _UserService = userService;
            _Logger = logger;
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
                _Logger.LogInformation("Savings pot {PotId} deleted by user {UserId}", potId, userId);
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "Error deleting savings pot {PotId}", potId);
                result.Successful = false;
                result.ErrorMessage = "Something went wrong while deleting the savings pot.";
            }

            return result;
        }
    }
}
