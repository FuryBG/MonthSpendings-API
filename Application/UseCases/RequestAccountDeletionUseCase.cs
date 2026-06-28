using Application.Contracts;
using Application.Interfaces;
using Application.Services;
using Domain;
using Microsoft.Extensions.Logging;

namespace Application.UseCases
{
    public interface IRequestAccountDeletionUseCase
    {
        Task<CaseResult<bool>> InvokeAsync();
    }

    public class RequestAccountDeletionUseCase : IRequestAccountDeletionUseCase
    {
        private readonly IUnitOfWork _UnitOfWork;
        private readonly IUserService _UserService;
        private readonly ILogger<RequestAccountDeletionUseCase> _Logger;

        public RequestAccountDeletionUseCase(IUnitOfWork unitOfWork, IUserService userService, ILogger<RequestAccountDeletionUseCase> logger)
        {
            _UnitOfWork = unitOfWork;
            _UserService = userService;
            _Logger = logger;
        }

        public async Task<CaseResult<bool>> InvokeAsync()
        {
            var result = new CaseResult<bool>();

            try
            {
                int userId = _UserService.GetUserId();

                var existing = await _UnitOfWork.AccountDeleteRequestRepository.GetPendingByUserIdAsync(userId);
                if (existing != null)
                {
                    result.Successful = true;
                    result.Data = true;
                    return result;
                }

                var request = new AccountDeleteRequest { UserId = userId };
                _UnitOfWork.AccountDeleteRequestRepository.Add(request);
                await _UnitOfWork.CommitAsync();

                _Logger.LogInformation("Account deletion requested by user {UserId}", userId);
                result.Successful = true;
                result.Data = true;
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "Error creating account deletion request");
                result.Successful = false;
                result.ErrorMessage = "Failed to submit deletion request. Please try again later.";
            }

            return result;
        }
    }
}
