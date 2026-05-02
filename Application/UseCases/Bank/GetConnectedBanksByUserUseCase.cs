using Application.Contracts;
using Application.Dto.Bank;
using Application.Interfaces;
using Application.Mappers;
using Application.Services;
using Domain.Bank;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.Bank
{
    public class GetConnectedBanksByUserUseCase
    {
        private IUnitOfWork _UnitOfWork { get; set; }
        private IUserService _UserService { get; set; }
        private readonly ILogger<GetConnectedBanksByUserUseCase> _Logger;

        public GetConnectedBanksByUserUseCase(IUnitOfWork unitOfWork, IUserService userService, ILogger<GetConnectedBanksByUserUseCase> logger)
        {
            _UnitOfWork = unitOfWork;
            _UserService = userService;
            _Logger = logger;
        }

        public async Task<CaseResult<List<BankConsentDto>>> InvokeAsync()
        {
            var result = new CaseResult<List<BankConsentDto>>();
            result.Successful = true;

            int userId = 0;
            try
            {
                userId = _UserService.GetUserId();
                List<BankConsent> connectedBanks = await _UnitOfWork.BankConsentRepository.GetBankConsentByUserId(userId);
                List<BankConsentDto> connectedBanksDto = connectedBanks.Select(connectedBank => connectedBank.ToDto()).ToList();
                result.Data = connectedBanksDto;
                _Logger.LogInformation("Retrieved {Count} connected banks for user {UserId}", result.Data!.Count, userId);
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "Error retrieving connected banks for user {UserId}", userId);
                result.Successful = false;
                result.ErrorMessage = "Something got wrong during loading of connected banks. Please try again later.";
            }
            return result;
        }
    }
}
