using Application.BackgroundWorkers;

namespace MonthSpendings.BackgroundServices
{
    public class TransactionSyncBackgroundService : BackgroundService
    {
        private IServiceScopeFactory _ScopeFactory { get; set; }
        private double _IntervalInMinutes { get; set; }
        private ILogger _Logger { get; set; }

        public TransactionSyncBackgroundService(IServiceScopeFactory scopeFactory, ILogger<TransactionSyncBackgroundService> logger, IConfiguration configuration)
        {
            _ScopeFactory = scopeFactory;
            _Logger = logger;
            bool intervalSet = double.TryParse(configuration.GetSection("EnableBanking:TransactionSyncIntervalInMinutes").Value, out double updateInterval);

            if (intervalSet == false)
            {
                throw new Exception("EnableBanking:TransactionSyncIntervalInMinutes is not set.");
            }
            _IntervalInMinutes = updateInterval;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _Logger.LogInformation("TransactionSyncBackgroundService started — interval: {IntervalMinutes} min", _IntervalInMinutes);
            //await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await using var scope = _ScopeFactory.CreateAsyncScope();
                    var bankSyncWorker = scope.ServiceProvider.GetRequiredService<IBankSyncWorker>();
                    await bankSyncWorker.SyncBankAccountsAsync(stoppingToken);
                    await Task.Delay(TimeSpan.FromMinutes(_IntervalInMinutes), stoppingToken);
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