using Application.Contracts;
using Application.Dto;
using Application.Interfaces;
using Application.Services;
using Domain;
using Microsoft.Extensions.Logging;

namespace Application.UseCases
{
    public interface IUpdateLastUserActivityUseCase
    {
        Task<CaseResult<bool>> InvokeAsync(UpdateUserActivityDto dto);
    }

    public class UpdateLastUserActivityUseCase : IUpdateLastUserActivityUseCase
    {
        private IUnitOfWork _UnitOfWork { get; set; }
        private IUserService _UserService { get; set; }
        private readonly ILogger<UpdateLastUserActivityUseCase> _Logger;

        public UpdateLastUserActivityUseCase(IUnitOfWork unitOfWork, IUserService userService, ILogger<UpdateLastUserActivityUseCase> logger)
        {
            _UnitOfWork = unitOfWork;
            _UserService = userService;
            _Logger = logger;
        }

        public async Task<CaseResult<bool>> InvokeAsync(UpdateUserActivityDto dto)
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

                user.LastVisited = DateTime.UtcNow;
                user.Timezone = dto.Timezone;
                await _UnitOfWork.CommitAsync();

                result.Data = true;
                _Logger.LogInformation("LastVisited updated for user {UserId}, timezone: {Timezone}", userId, dto.Timezone);
            }
            catch (Exception ex)
            {
                result.Successful = false;
                result.ErrorMessage = "Failed to update user activity.";
                _Logger.LogError(ex, "Error updating last activity for user {UserId}", _UserService.GetUserId());
            }

            return result;
        }
    }
}
