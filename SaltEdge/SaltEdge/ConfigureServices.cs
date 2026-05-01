using Microsoft.Extensions.DependencyInjection;
using SaltEdge.Handlers;
using SaltEdge.Interfaces;
using SaltEdge.Services;

namespace SaltEdge
{
    public static class ConfigureServices
    {
        public static IServiceCollection AddSaltEdgeApi(this IServiceCollection services, Action<SaltEdgeOptions> configureOptions)
        {
            services.Configure(configureOptions);
            services.PostConfigure<SaltEdgeOptions>(options =>
            {
                if (!options.BaseUrl.EndsWith('/'))
                {
                    options.BaseUrl += "/";
                }
            });

            services.AddTransient<SaltEdgeAuthHandler>();

            services.AddHttpClient<ICustomersService, CustomersService>("SaltEdgeCustomers", ConfigureHttpClient)
                .AddHttpMessageHandler<SaltEdgeAuthHandler>();
            services.AddHttpClient<IProvidersService, ProvidersService>("SaltEdgeProviders", ConfigureHttpClient)
                .AddHttpMessageHandler<SaltEdgeAuthHandler>();
            services.AddHttpClient<IConnectionsService, ConnectionsService>("SaltEdgeConnections", ConfigureHttpClient)
                .AddHttpMessageHandler<SaltEdgeAuthHandler>();
            services.AddHttpClient<IAccountsService, AccountsService>("SaltEdgeAccounts", ConfigureHttpClient)
                .AddHttpMessageHandler<SaltEdgeAuthHandler>();
            services.AddHttpClient<ITransactionsService, TransactionsService>("SaltEdgeTransactions", ConfigureHttpClient)
                .AddHttpMessageHandler<SaltEdgeAuthHandler>();

            return services;
        }

        private static void ConfigureHttpClient(IServiceProvider serviceProvider, HttpClient client)
        {
            SaltEdgeOptions options = serviceProvider.GetRequiredService<Microsoft.Extensions.Options.IOptions<SaltEdgeOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
        }
    }
}
