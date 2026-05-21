using Application.Contracts;
using EnableBanking.Interfaces;
using EnableBanking.Models;
using EnableBanking.Models.General;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.Bank
{
    public interface IGetBanksUseCase
    {
        Task<CaseResult<List<Aspsp>?>> InvokeAsync(string? bankName);
    }

    public class GetBanksUseCase : IGetBanksUseCase
    {
        private const string CacheKey = "enablebanking:aspsps";
        private static readonly TimeSpan CacheTtl = TimeSpan.FromHours(1);

        private readonly IGeneralService _GeneralService;
        private readonly IMemoryCache _Cache;
        private readonly ILogger<GetBanksUseCase> _Logger;

        public GetBanksUseCase(IGeneralService generalService, IMemoryCache cache, ILogger<GetBanksUseCase> logger)
        {
            _GeneralService = generalService;
            _Cache = cache;
            _Logger = logger;
        }

        public async Task<CaseResult<List<Aspsp>?>> InvokeAsync(string? bankName)
        {
            var result = new CaseResult<List<Aspsp>?>();
            result.Successful = true;

            try
            {
                if (!_Cache.TryGetValue(CacheKey, out List<Aspsp>? allAspsps))
                {
                    ApiResponse<GetASPSPsResponse> aspsResponse = await _GeneralService.GetASPSPsAsync(new GetASPSPsRequest(), new CancellationToken());

                    if (aspsResponse.Error != null)
                    {
                        _Logger.LogError("EnableBanking GetBanks failed: {ErrorDetail}", aspsResponse.Error.Detail);
                        result.ErrorMessage = aspsResponse.Error.Message;
                        result.Successful = false;
                        return result;
                    }

                    allAspsps = aspsResponse.Data?.Aspsps?.ToList() ?? [];
                    _Cache.Set(CacheKey, allAspsps, CacheTtl);
                }

                result.Data = bankName == null
                    ? allAspsps
                    : allAspsps!.Where(b => b.Name != null && b.Name.Contains(bankName, StringComparison.OrdinalIgnoreCase)).ToList();

                _Logger.LogInformation("Retrieved {Count} banks for query '{Query}'", result.Data?.Count, bankName);
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "Error retrieving banks");
                result.Successful = false;
                result.ErrorMessage = "Something got wrong during getting banks. Please try again later.";
            }
            return result;
        }
    }
}
