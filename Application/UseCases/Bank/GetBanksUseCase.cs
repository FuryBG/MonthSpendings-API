using Application.Contracts;
using EnableBanking.Interfaces;
using EnableBanking.Models;
using EnableBanking.Models.General;

namespace Application.UseCases.Bank
{
    public interface IGetBanksUseCase
    {
        Task<CaseResult<List<Aspsp>?>> InvokeAsync(string? bankName);
    }

    public class GetBanksUseCase : IGetBanksUseCase
    {
        private IGeneralService _GeneralService;

        public GetBanksUseCase(IGeneralService generalService)
        {
            _GeneralService = generalService;
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
                    Console.WriteLine($"Get Banks wrong response: {aspsResponse.Error.Detail}");
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
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                result.Successful = false;
                result.ErrorMessage = "Something got wrong during getting banks. Please try again later.";
            }
            return result;
        }
    }
}
