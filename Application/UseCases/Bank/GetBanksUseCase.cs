using Application.Contracts;
using EnableBanking.Interfaces;
using EnableBanking.Models;
using EnableBanking.Models.General;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.Bank
{
    public interface IGetBanksUseCase
    {
        Task<CaseResult<List<Aspsp>?>> InvokeAsync(string? bankName);
    }

    public class GetBanksUseCase : IGetBanksUseCase
    {
        private IGeneralService _GeneralService;
        private readonly ILogger<GetBanksUseCase> _Logger;

        public GetBanksUseCase(IGeneralService generalService, ILogger<GetBanksUseCase> logger)
        {
            _GeneralService = generalService;
            _Logger = logger;
        }

        public async Task<CaseResult<List<Aspsp>?>> InvokeAsync(string? bankName)
        {
            var result = new CaseResult<List<Aspsp>?>();
            result.Successful = true;

            try
            {
                ApiResponse<GetASPSPsResponse> aspsResponse = await _GeneralService.GetASPSPsAsync(new GetASPSPsRequest(), new CancellationToken());

                if (aspsResponse.Error != null)
                {
                    _Logger.LogError("EnableBanking GetBanks failed: {ErrorDetail}", aspsResponse.Error.Detail);
                    result.ErrorMessage = aspsResponse.Error.Message;
                    result.Successful = false;
                    return result;
                }

                if (bankName != null)
                {
                    result.Data = aspsResponse.Data?.Aspsps?
                    .Where(bank => bank.Name != null && bank.Name.ToLower().Contains(bankName.ToLower()))
                    .ToList();
                }
                else
                {
                    result.Data = aspsResponse.Data?.Aspsps?.ToList();
                }

                _Logger.LogInformation("Retrieved {Count} banks for query '{Query}'", result.Data!.Count, bankName);
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
