using Application.UseCases.Bank;

namespace MonthSpendings.BackgroundServices
{
    public class BankCacheWarmupService : BackgroundService
    {
        private readonly IServiceScopeFactory _ScopeFactory;
        private readonly ILogger<BankCacheWarmupService> _Logger;

        public BankCacheWarmupService(IServiceScopeFactory scopeFactory, ILogger<BankCacheWarmupService> logger)
        {
            _ScopeFactory = scopeFactory;
            _Logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                using var scope = _ScopeFactory.CreateScope();
                var getBanksUseCase = scope.ServiceProvider.GetRequiredService<IGetBanksUseCase>();
                var result = await getBanksUseCase.InvokeAsync(null, stoppingToken);

                if (result.Successful)
                {
                    _Logger.LogInformation("Bank list cache pre-warmed with {Count} banks", result.Data?.Count ?? 0);
                }
                else
                {
                    _Logger.LogWarning("Bank list cache warmup failed: {Error}", result.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "Error pre-warming bank list cache");
            }
        }
    }
}
