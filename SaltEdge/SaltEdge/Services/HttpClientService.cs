using System.Text;
using System.Text.Json;

namespace SaltEdge.Services
{
    public abstract class HttpClientService
    {
        private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        private readonly HttpClient _httpClient;

        protected HttpClientService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        protected async Task<Models.ApiResponse<T>> GetAsync<T>(string requestUri, CancellationToken cancellationToken)
        {
            HttpResponseMessage responseMessage = await _httpClient.GetAsync(requestUri, cancellationToken);
            return await HandleResponse<T>(responseMessage, cancellationToken);
        }

        protected async Task<Models.ApiResponse<T>> PostAsync<T>(string requestUri, object requestBody, CancellationToken cancellationToken)
        {
            string jsonContent = JsonSerializer.Serialize(requestBody, _jsonOptions);
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            HttpResponseMessage responseMessage = await _httpClient.PostAsync(requestUri, httpContent, cancellationToken);

            return await HandleResponse<T>(responseMessage, cancellationToken);
        }

        protected async Task<Models.ApiResponse<T>> DeleteAsync<T>(string requestUri, CancellationToken cancellationToken)
        {
            HttpResponseMessage responseMessage = await _httpClient.DeleteAsync(requestUri, cancellationToken);
            return await HandleResponse<T>(responseMessage, cancellationToken);
        }

        private static async Task<Models.ApiResponse<T>> HandleResponse<T>(HttpResponseMessage responseMessage, CancellationToken cancellationToken)
        {
            string content = await responseMessage.Content.ReadAsStringAsync(cancellationToken);
            var response = string.IsNullOrWhiteSpace(content)
                ? new Models.ApiResponse<T>()
                : JsonSerializer.Deserialize<Models.ApiResponse<T>>(content, _jsonOptions) ?? new Models.ApiResponse<T>();

            response.StatusCode = responseMessage.StatusCode;
            return response;
        }
    }
}
