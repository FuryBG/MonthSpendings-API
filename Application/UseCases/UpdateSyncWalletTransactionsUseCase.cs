using Application.Contracts;
using Application.Dto;
using Application.Interfaces;
using Application.Services;
using Domain;
using Microsoft.Extensions.Logging;

namespace Application.UseCases
{
    public interface IUpdateSyncWalletTransactionsUseCase
    {
        Task<CaseResult<bool>> InvokeAsync(UpdateSyncWalletTransactionsDto dto);
    }

    public class UpdateSyncWalletTransactionsUseCase : IUpdateSyncWalletTransactionsUseCase
    {
        private IUnitOfWork _UnitOfWork { get; set; }
        private IUserService _UserService { get; set; }
        private readonly ILogger<UpdateSyncWalletTransactionsUseCase> _Logger;

        public UpdateSyncWalletTransactionsUseCase(IUnitOfWork unitOfWork, IUserService userService, ILogger<UpdateSyncWalletTransactionsUseCase> logger)
        {
            _UnitOfWork = unitOfWork;
            _UserService = userService;
            _Logger = logger;
        }

        public async Task<CaseResult<bool>> InvokeAsync(UpdateSyncWalletTransactionsDto dto)
        {
            var result = new CaseResult<bool>();
            result.Successful = true;

            try
            {
                int userId = _UserService.GetUserId();
                AppUser? user = await _UnitOfWork.UserRepository.GetUserById(userId);

                if (user == null)
                {
                    result.Successful = false;
                    result.ErrorMessage = "Invalid user.";
                    return result;
                }

                if (!user.IsPro)
                {
                    result.Successful = false;
                    result.ErrorMessage = "Feature requires an active Pro subscription.";
                    return result;
                }

                user.SyncWalletTransactions = dto.SyncWalletTransactions;
                await _UnitOfWork.CommitAsync();

                result.Data = true;
                _Logger.LogInformation("SyncWalletTransactions set to {Value} for user {UserId}.", dto.SyncWalletTransactions, userId);
            }
            catch (Exception ex)
            {
                result.Successful = false;
                result.ErrorMessage = "Failed to update wallet sync setting.";
                _Logger.LogError(ex, "Error updating SyncWalletTransactions for user {UserId}.", _UserService.GetUserId());
            }

            return result;
        }
    }
}
