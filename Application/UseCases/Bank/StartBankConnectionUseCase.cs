using Application.Contracts;
using Application.Interfaces;
using Application.Services;
using Domain;
using Domain.Bank;
using EnableBanking.Interfaces;
using EnableBanking.Models;
using EnableBanking.Models.General;

namespace Application.UseCases.Bank
{
    public interface IStartBankConnectionUseCase
    {
        Task<CaseResult<string?>> InvokeAsync(string bankName, string countryCode, string bankImageUrl, int maximumConsentValidity);
    }

    public class StartBankConnectionUseCase : IStartBankConnectionUseCase
    {
        private IUnitOfWork _UnitOfWork { get; set; }
        private IUserService _UserService { get; set; }
        private IGeneralService _GeneralService;

        public StartBankConnectionUseCase(IUnitOfWork unitOfWork, IUserService userService, IGeneralService generalService)
        {
            _UnitOfWork = unitOfWork;
            _UserService = userService;
            _GeneralService = generalService;
        }

        public async Task<CaseResult<string?>> InvokeAsync(string bankName, string countryCode, string bankImageUrl, int maximumConsentValidity)
        {
            var result = new CaseResult<string?>();
            result.Successful = true;

            try
            {
                int userId = _UserService.GetUserId();
                AppUser? existingUser = await _UnitOfWork.UserRepository.GetUserById(userId);
                Guid sessionId = Guid.NewGuid();

                if (existingUser == null)
                {
                    Console.WriteLine($"Unauthorized user tries to connect bank.");
                    result.Successful = false;
                    result.ErrorMessage = "Can't find your personal information. Please first login, to connect youyr bank.";
                    return result;
                }

                ApiResponse<StartAuthorizationResponse> resp = await _GeneralService.StartAuthorizationAsync(new StartAuthorizationRequest()
                {
                    Access = new Access { ValidUntil = DateTime.UtcNow.AddSeconds(maximumConsentValidity - 5), Balances = true, Transactions = true },
                    PsuType = "personal",
                    State = sessionId.ToString(),
                    Language = "en",
                    RedirectUrl = new Uri($"https://355d-88-203-208-219.ngrok-free.app/api/Bank/connect-callback"),
                    CredentialsAutosubmit = true,
                    Aspsp = new Aspsp()
                    {
                        Name = bankName,
                        Country = countryCode
                    }
                }, new CancellationToken());

                if (resp.StatusCode != System.Net.HttpStatusCode.OK || resp.Data == null || resp.Data.Url == null || resp.Data.AuthorizationId == null)
                {
                    Console.WriteLine(resp.Error?.Detail);
                    result.Successful = false;
                    result.ErrorMessage = resp.Error?.Message;
                    return result;
                }

                BankConsent consent = new BankConsent()
                {
                    BankName = bankName,
                    CountryCode = countryCode,
                    SessionId = sessionId,
                    BankImgUrl = bankImageUrl,
                    UserId = existingUser.Id,
                    ExpiresOn = DateTime.UtcNow.AddSeconds(maximumConsentValidity - 5)
                };

                await _UnitOfWork.BankConsentRepository.CreateBankConsent(consent);
                await _UnitOfWork.CommitAsync();
                result.Data = resp.Data.Url.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                result.Successful = false;
                result.ErrorMessage = "Something got wrong during starting bank connection. Please try again later.";
            }
            return result;
        }
    }
}
