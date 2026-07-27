using Application.Contracts;
using Application.Dto;
using Application.Interfaces;
using Application.Services;
using Domain;
using Microsoft.Extensions.Logging;

namespace Application.UseCases
{
    public interface IUpdateNotificationTokenUseCase
    {
        Task<CaseResult<bool>> InvokeAsync(UpdateNotificationTokenDto dto);
    }

    public class UpdateNotificationTokenUseCase : IUpdateNotificationTokenUseCase
    {
        private IUnitOfWork _UnitOfWork { get; set; }
        private IUserService _UserService { get; set; }
        private readonly ILogger<UpdateNotificationTokenUseCase> _Logger;

        public UpdateNotificationTokenUseCase(IUnitOfWork unitOfWork, IUserService userService, ILogger<UpdateNotificationTokenUseCase> logger)
        {
            _UnitOfWork = unitOfWork;
            _UserService = userService;
            _Logger = logger;
        }

        public async Task<CaseResult<bool>> InvokeAsync(UpdateNotificationTokenDto dto)
        {
            var result = new CaseResult<bool>();
            result.Successful = true;

            try
            {
                int userId = _UserService.GetUserId();
                AppUser? user = await _UnitOfWork.UserRepository.GetUserById(userId);

                if (user == null)
                {
                    result.Successful = false;
                    result.ErrorMessage = "Invalid user.";
                    return result;
                }

                if (user.NotificationToken == dto.NotificationToken)
                {
                    result.Data = true;
                    _Logger.LogInformation("Notification token unchanged for user {UserId}, skipping update.", userId);
                    return result;
                }

                user.NotificationToken = dto.NotificationToken;
                await _UnitOfWork.CommitAsync();

                result.Data = true;
                _Logger.LogInformation("Notification token updated for user {UserId}.", userId);
            }
            catch (Exception ex)
            {
                result.Successful = false;
                result.ErrorMessage = "Failed to update notification token.";
                _Logger.LogError(ex, "Error updating notification token.");
            }

            return result;
        }
    }
}
