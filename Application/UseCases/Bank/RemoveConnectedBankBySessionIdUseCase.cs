using Application.Contracts;
using Application.Interfaces;
using Application.Services;
using Domain.Bank;

namespace Application.UseCases.Bank
{
    public interface IRemoveConnectedBankBySessionIdUseCase
    {
        Task<CaseResult<bool>> InvokeAsync(Guid sessionId, CancellationToken cancellationToken);
    }

    public class RemoveConnectedBankBySessionIdUseCase : IRemoveConnectedBankBySessionIdUseCase
    {
        private IUnitOfWork _UnitOfWork { get; set; }
        private IUserService _UserService { get; set; }

        public RemoveConnectedBankBySessionIdUseCase(IUnitOfWork unitOfWork, IUserService userService)
        {
            _UnitOfWork = unitOfWork;
            _UserService = userService;
        }

        public async Task<CaseResult<bool>> InvokeAsync(Guid sessionId, CancellationToken cancellationToken)
        {
            var result = new CaseResult<bool>();
            result.Successful = true;

            try
            {
                int userId = _UserService.GetUserId();
                int deleted = await _UnitOfWork.BankConsentRepository.Delete(((BankConsent bankConsent) =>
                    bankConsent.SessionId == sessionId &&
                    bankConsent.UserId == userId), cancellationToken);

                if (deleted <= 0)
                {
                    Console.WriteLine($"Can't delete bank consent with session id:  {sessionId} for user with Id: {userId}");
                    result.Successful = false;
                    result.ErrorMessage = "Something got wrong during remove bank account. Please try again later.";
                    return result;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                result.Successful = false;
                result.ErrorMessage = "Something got wrong during remove bank account. Please try again later.";
            }
            return result;
        }
    }
}