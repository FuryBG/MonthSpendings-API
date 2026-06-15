using Application.Contracts;
using Application.Dto.Bank;
using Application.Mappers;
using EnableBanking.Interfaces;
using EnableBanking.Models;
using EnableBanking.Models.General;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.Bank
{
    public interface IGetBanksUseCase
    {
        Task<CaseResult<List<BankOptionDto>>> InvokeAsync(string? bankName, CancellationToken cancellationToken);
    }

    public class GetBanksUseCase : IGetBanksUseCase
    {
        public const string CacheKey = "enablebanking:aspsps";
        public static readonly TimeSpan CacheTtl = TimeSpan.FromHours(1);

        private readonly IGeneralService _GeneralService;
        private readonly IMemoryCache _Cache;
        private readonly ILogger<GetBanksUseCase> _Logger;

        public GetBanksUseCase(IGeneralService generalService, IMemoryCache cache, ILogger<GetBanksUseCase> logger)
        {
            _GeneralService = generalService;
            _Cache = cache;
            _Logger = logger;
        }

        public async Task<CaseResult<List<BankOptionDto>>> InvokeAsync(string? bankName, CancellationToken cancellationToken)
        {
            var result = new CaseResult<List<BankOptionDto>>();
            result.Successful = true;

            try
            {
                if (!_Cache.TryGetValue(CacheKey, out List<BankOptionDto>? allBanks))
                {
                    // Populates a shared cache, so it must not be aborted by this caller's cancellation.
                    ApiResponse<GetASPSPsResponse> aspsResponse = await _GeneralService.GetASPSPsAsync(new GetASPSPsRequest(), CancellationToken.None);

                    if (aspsResponse.Error != null)
                    {
                        _Logger.LogError("EnableBanking GetBanks failed: {ErrorDetail}", aspsResponse.Error.Detail);
                        result.ErrorMessage = aspsResponse.Error.Message;
                        result.Successful = false;
                        return result;
                    }

                    allBanks = aspsResponse.Data?.Aspsps?.Select(a => a.ToDto()).ToList() ?? [];
                    _Cache.Set(CacheKey, allBanks, CacheTtl);
                }

                result.Data = bankName == null
                    ? allBanks!.ToList()
                    : allBanks!.Where(b => b.Name.Contains(bankName, StringComparison.OrdinalIgnoreCase)).ToList();

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
