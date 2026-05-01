using Application.BackgroundWorkers;

namespace MonthSpendings.BackgroundServices
{
    public class SaltEdgeTransactionSyncBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly double _intervalInMinutes;
        private readonly ILogger _logger;

        public SaltEdgeTransactionSyncBackgroundService(IServiceScopeFactory scopeFactory, ILogger<SaltEdgeTransactionSyncBackgroundService> logger, IConfiguration configuration)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            bool intervalSet = double.TryParse(configuration.GetSection("SaltEdge:TransactionSyncIntervalInMinutes").Value, out double updateInterval);

            if (!intervalSet)
            {
                throw new Exception("SaltEdge:TransactionSyncIntervalInMinutes is not set.");
            }

            _intervalInMinutes = updateInterval;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await using var scope = _scopeFactory.CreateAsyncScope();
                    var worker = scope.ServiceProvider.GetRequiredService<ISaltEdgeSyncWorker>();
                    await worker.SyncBankAccountsAsync(stoppingToken);
                    await Task.Delay(TimeSpan.FromMinutes(_intervalInMinutes), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Exception during Salt Edge bank sync.");
                }
            }
        }
    }
}
