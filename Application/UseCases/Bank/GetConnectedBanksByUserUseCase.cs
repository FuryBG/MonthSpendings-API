using Application.Contracts;
using Application.Dto.Bank;
using Application.Interfaces;
using Application.Mappers;
using Application.Services;
using Domain.Bank;

namespace Application.UseCases.Bank
{
    public class GetConnectedBanksByUserUseCase
    {
        private IUnitOfWork _UnitOfWork { get; set; }
        private IUserService _UserService { get; set; }
        public GetConnectedBanksByUserUseCase(IUnitOfWork unitOfWork, IUserService userService)
        {
            _UnitOfWork = unitOfWork;
            _UserService = userService;
        }

        public async Task<CaseResult<List<BankConsentDto>>> InvokeAsync()
        {
            var result = new CaseResult<List<BankConsentDto>>();
            result.Successful = true;

            try
            {
                int userId = _UserService.GetUserId();
                List<BankConsent> connectedBanks = await _UnitOfWork.BankConsentRepository.GetBankConsentByUserId(userId);
                List<BankConsentDto> connectedBanksDto = connectedBanks.Select(connectedBank => connectedBank.ToDto()).ToList();
                result.Data = connectedBanksDto;
            }
            catch (Exception ex)
            {
                result.Successful = false;
                result.ErrorMessage = "Something got wrong during loading of connected banks. Please try again later.";
                Console.WriteLine(ex);
            }
            return result;
        }
    }
}
