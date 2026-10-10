using Application.Resources;
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

            int userId = _UserService.GetUserId();
            AppUser? user = await _UnitOfWork.UserRepository.GetUserById(userId);

            if (user == null)
            {
                _Logger.LogWarning("User {UserId} not found when updating wallet sync", userId);
                return CaseResult<bool>.Error(Messages.UserInvalid);
            }

            if (!user.IsPro)
            {
                _Logger.LogInformation("Non-pro user {UserId} tried to change wallet sync", userId);
                return CaseResult<bool>.Error(Messages.SubscriptionProRequired, ErrorType.ProRequired);
            }

            user.SyncWalletTransactions = dto.SyncWalletTransactions;
            await _UnitOfWork.CommitAsync();

            result.Data = true;
            _Logger.LogInformation("SyncWalletTransactions set to {Value} for user {UserId}.", dto.SyncWalletTransactions, userId);

            return result;
        }
    }
}
