using Application.Interfaces;
using Application.Interfaces.Repository.SaltEdge;
using Domain.SaltEdge;
using Domain.SaltEdge.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SaltEdge.Interfaces;
using SaltEdge.Models.Accounts;
using SaltEdge.Models.Connections;
using SaltEdge.Models.Transactions;
using System.Net;

namespace Application.BackgroundWorkers
{
    public interface ISaltEdgeSyncWorker
    {
        Task SyncBankAccountsAsync(CancellationToken cancellationToken);
    }

    public sealed class SaltEdgeSyncWorker : ISaltEdgeSyncWorker
    {
        private readonly IConnectionsService _connectionsService;
        private readonly IAccountsService _accountsService;
        private readonly ITransactionsService _transactionsService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<SaltEdgeSyncWorker> _logger;
        private readonly IConfiguration _configuration;

        public SaltEdgeSyncWorker(
            IConnectionsService connectionsService,
            IAccountsService accountsService,
            ITransactionsService transactionsService,
            IUnitOfWork unitOfWork,
            IConfiguration configuration,
            ILogger<SaltEdgeSyncWorker> logger)
        {
            _connectionsService = connectionsService;
            _accountsService = accountsService;
            _transactionsService = transactionsService;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SyncBankAccountsAsync(CancellationToken cancellationToken)
        {
            bool intervalSet = int.TryParse(_configuration.GetSection("SaltEdge:TransactionSyncIntervalInMinutes").Value, out int updateInterval);

            if (!intervalSet)
            {
                throw new ApplicationException("SaltEdge:TransactionSyncIntervalInMinutes is not set.");
            }

            DateTime threshold = DateTime.UtcNow.AddMinutes(-updateInterval);
            List<SaltEdgeConnection> connections = await _unitOfWork.SaltEdgeConnectionRepository.GetConnectionsForSyncAsync(threshold, cancellationToken);

            try
            {
                foreach (SaltEdgeConnection connection in connections)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    if (string.IsNullOrWhiteSpace(connection.ConnectionId))
                    {
                        continue;
                    }

                    var connectionResponse = await _connectionsService.GetAsync(connection.ConnectionId, cancellationToken);
                    if (connectionResponse.StatusCode != HttpStatusCode.OK || connectionResponse.Data == null)
                    {
                        _logger.LogWarning("Cannot load Salt Edge connection {ConnectionId}.", connection.ConnectionId);
                        continue;
                    }

                    if (!string.Equals(connectionResponse.Data.Status, "active", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    var accountsResponse = await _accountsService.GetAsync(connection.ConnectionId, cancellationToken);
                    if (accountsResponse.StatusCode != HttpStatusCode.OK || accountsResponse.Data == null)
                    {
                        _logger.LogWarning("Cannot load Salt Edge accounts for connection {ConnectionId}.", connection.ConnectionId);
                        continue;
                    }

                    foreach (SaltEdgeAccount account in connection.Accounts)
                    {
                        AccountResponse? accountResponse = accountsResponse.Data.FirstOrDefault(a => a.Id == account.AccountId);
                        if (accountResponse == null)
                        {
                            continue;
                        }

                        var postedTransactionsResponse = await _transactionsService.GetAsync(connection.ConnectionId, null, false, cancellationToken);
                        if (postedTransactionsResponse.StatusCode != HttpStatusCode.OK || postedTransactionsResponse.Data == null)
                        {
                            _logger.LogWarning("Cannot load Salt Edge transactions for connection {ConnectionId}.", connection.ConnectionId);
                            continue;
                        }

                        List<TransactionResponse> expenseTransactions = postedTransactionsResponse.Data
                            .Where(t => t.AccountId == account.AccountId && t.Amount < 0)
                            .ToList();

                        List<string> transactionIds = expenseTransactions
                            .Where(t => !string.IsNullOrWhiteSpace(t.Id))
                            .Select(t => BuildTransactionId(connection.ConnectionId, account.AccountId, t))
                            .ToList();

                        HashSet<string> existingIds = await _unitOfWork.SaltEdgeTransactionRepository.GetExistingTransactionIdsAsync(transactionIds, cancellationToken);

                        await _unitOfWork.BeginTransactionAsync();

                        foreach (TransactionResponse transaction in expenseTransactions)
                        {
                            if (string.IsNullOrWhiteSpace(transaction.Id) || transaction.MadeOn == null)
                            {
                                continue;
                            }

                            string transactionId = BuildTransactionId(connection.ConnectionId, account.AccountId, transaction);
                            if (existingIds.Contains(transactionId))
                            {
                                continue;
                            }

                            await _unitOfWork.SaltEdgeTransactionRepository.AddTransactionAsync(new SaltEdgeTransaction()
                            {
                                TransactionId = transactionId,
                                ExternalTransactionId = transaction.Id,
                                SaltEdgeAccountId = account.Id,
                                Currency = transaction.CurrencyCode ?? account.Currency,
                                Amount = Math.Abs(transaction.Amount),
                                MerchantCode = transaction.Extra?.MerchantCategoryCode,
                                Description = transaction.Description,
                                Status = ParseStatus(transaction.Status),
                                BookingDate = transaction.MadeOn.Value.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
                            }, cancellationToken);
                        }

                        await _unitOfWork.CommitTransactionAsync();
                    }

                    await _unitOfWork.SaltEdgeConnectionRepository.MarkConnectionsAsSyncedAsync([connection.Id], DateTime.UtcNow, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                await _unitOfWork.RollbackTransactionAsync();
            }
        }

        private static SaltEdgeTransactionStatus ParseStatus(string? status)
        {
            return string.Equals(status, "pending", StringComparison.OrdinalIgnoreCase)
                ? SaltEdgeTransactionStatus.Pending
                : SaltEdgeTransactionStatus.Posted;
        }

        private static string BuildTransactionId(string connectionId, string accountId, TransactionResponse transaction)
        {
            return $"{connectionId}/{accountId}/{transaction.Id}/{transaction.Amount}/{transaction.CurrencyCode}/{transaction.MadeOn:yyyy-MM-dd}";
        }
    }
}
