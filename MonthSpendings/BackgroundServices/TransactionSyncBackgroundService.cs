using Application.BackgroundWorkers;

namespace MonthSpendings.BackgroundServices
{
    public class TransactionSyncBackgroundService : BackgroundService
    {
        private IServiceScopeFactory _ScopeFactory { get; set; }
        private int _IntervalInMinutes { get; set; }
        private ILogger _Logger { get; set; }

        public TransactionSyncBackgroundService(IServiceScopeFactory scopeFactory, ILogger<TransactionSyncBackgroundService> logger, IConfiguration configuration)
        {
            _ScopeFactory = scopeFactory;
            _Logger = logger;
            bool intervalSet = int.TryParse(configuration.GetSection("EnableBanking:TransactionSyncInterval").Value, out int updateInterval);

            if (intervalSet == false)
            {
                throw new Exception("EnableBanking:TransactionSyncInterval is not set.");
            }
            _IntervalInMinutes = updateInterval;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await using var scope = _ScopeFactory.CreateAsyncScope();
                    var bankSyncWorker = scope.ServiceProvider.GetRequiredService<IBankSyncWorker>();
                    await bankSyncWorker.SyncBankAccountsAsync(stoppingToken);
                    await Task.Delay(TimeSpan.FromMinutes((double)_IntervalInMinutes), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _Logger.LogError(ex, "Exception during bank sync.");
                }
            }
        }
    }
}