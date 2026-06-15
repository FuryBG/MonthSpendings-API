using Application.Contracts;
using Application.Interfaces;
using Application.Services;
using Domain;
using Domain.Bank;
using EnableBanking.Interfaces;
using EnableBanking.Models;
using EnableBanking.Models.General;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

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
        private readonly IConfiguration _Configuration;
        private readonly ILogger<StartBankConnectionUseCase> _Logger;

        public StartBankConnectionUseCase(IUnitOfWork unitOfWork, IUserService userService, IGeneralService generalService, IConfiguration configuration, ILogger<StartBankConnectionUseCase> logger)
        {
            _UnitOfWork = unitOfWork;
            _UserService = userService;
            _GeneralService = generalService;
            _Configuration = configuration;
            _Logger = logger;
        }

        public async Task<CaseResult<string?>> InvokeAsync(string bankName, string countryCode, string bankImageUrl, int maximumConsentValidity)
        {
            var result = new CaseResult<string?>();
            result.Successful = true;

            int userId = 0;
            try
            {
                userId = _UserService.GetUserId();
                AppUser? existingUser = await _UnitOfWork.UserRepository.GetUserById(userId);
                Guid sessionId = Guid.NewGuid();

                if (existingUser == null)
                {
                    _Logger.LogWarning("Unauthorized bank connection attempt — no user ID");
                    result.Successful = false;
                    result.ErrorMessage = "Can't find your personal information. Please first login, to connect youyr bank.";
                    return result;
                }

                string? redirectUrl = _Configuration["EnableBanking:RedirectUrl"];
                if (string.IsNullOrWhiteSpace(redirectUrl))
                {
                    _Logger.LogError("EnableBanking:RedirectUrl is not configured");
                    result.Successful = false;
                    result.ErrorMessage = "Bank connection is not configured correctly. Please try again later.";
                    return result;
                }

                ApiResponse<StartAuthorizationResponse> resp = await _GeneralService.StartAuthorizationAsync(new StartAuthorizationRequest()
                {
                    Access = new Access { ValidUntil = DateTime.UtcNow.AddSeconds(maximumConsentValidity - 5), Balances = true, Transactions = true },
                    PsuType = "personal",
                    State = sessionId.ToString(),
                    Language = "en",
                    RedirectUrl = new Uri(redirectUrl),
                    Aspsp = new Aspsp()
                    {
                        Name = bankName,
                        Country = countryCode
                    }
                }, new CancellationToken());

                if (resp.StatusCode != System.Net.HttpStatusCode.OK || resp.Data == null || resp.Data.Url == null || resp.Data.AuthorizationId == null)
                {
                    _Logger.LogError("EnableBanking StartAuthorization failed: {ErrorDetail}", resp.Error?.Detail);
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
                _Logger.LogInformation("Bank connection initiated for user {UserId}", userId);
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "Error starting bank connection for user {UserId}", userId);
                result.Successful = false;
                result.ErrorMessage = "Something got wrong during starting bank connection. Please try again later.";
            }
            return result;
        }
    }
}
