using Application.BackgroundWorkers;
using Application.Contracts;
using Application.Services;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.Bank
{
    public interface ISyncTransactionsUseCase
    {
        Task<CaseResult<bool>> InvokeAsync(CancellationToken cancellationToken);
    }

    public class SyncTransactionsUseCase : ISyncTransactionsUseCase
    {
        private readonly IBankSyncWorker _BankSyncWorker;
        private readonly IUserService _UserService;
        private readonly ILogger<SyncTransactionsUseCase> _Logger;

        public SyncTransactionsUseCase(IBankSyncWorker bankSyncWorker, IUserService userService, ILogger<SyncTransactionsUseCase> logger)
        {
            _BankSyncWorker = bankSyncWorker;
            _UserService = userService;
            _Logger = logger;
        }

        public async Task<CaseResult<bool>> InvokeAsync(CancellationToken cancellationToken)
        {
            var result = new CaseResult<bool>();
            int userId = 0;
            try
            {
                userId = _UserService.GetUserId();
                await _BankSyncWorker.SyncBankAccountsByUserAsync(userId, cancellationToken);
                result.Successful = true;
                result.Data = true;
                _Logger.LogInformation("Manual transaction sync completed for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "Error during manual transaction sync for user {UserId}", userId);
                result.Successful = false;
                result.ErrorMessage = "Sync failed. Please try again later.";
            }
            return result;
        }
    }
}
