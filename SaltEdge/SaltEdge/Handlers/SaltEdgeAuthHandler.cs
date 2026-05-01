using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

namespace SaltEdge.Handlers
{
    public class SaltEdgeAuthHandler : DelegatingHandler
    {
        private readonly SaltEdgeOptions _options;

        public SaltEdgeAuthHandler(IOptions<SaltEdgeOptions> options)
        {
            _options = options.Value;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Headers.TryAddWithoutValidation("App-id", _options.AppId);
            request.Headers.TryAddWithoutValidation("Secret", _options.Secret);

            return base.SendAsync(request, cancellationToken);
        }
    }
}
