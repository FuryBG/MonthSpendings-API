using Application.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;

namespace MonthSpendings.Filters
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public sealed class RequireRevenueCatSecretAttribute : Attribute, IFilterFactory
    {
        public bool IsReusable => false;

        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
            => serviceProvider.GetRequiredService<RevenueCatAuthFilter>();
    }

    public sealed class RevenueCatAuthFilter : IAuthorizationFilter
    {
        private readonly RevenueCatOptions _Options;
        private readonly ILogger<RevenueCatAuthFilter> _Logger;

        public RevenueCatAuthFilter(IOptions<RevenueCatOptions> options, ILogger<RevenueCatAuthFilter> logger)
        {
            _Options = options.Value;
            _Logger = logger;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var received = context.HttpContext.Request.Headers.TryGetValue("Authorization", out var header)
                ? header.ToString()
                : null;

            if (received is null || received != _Options.WebhookSecret)
            {
                _Logger.LogWarning("RevenueCat webhook: unauthorized request from {IP}",
                    context.HttpContext.Connection.RemoteIpAddress);
                context.Result = new UnauthorizedResult();
            }
        }
    }
}
