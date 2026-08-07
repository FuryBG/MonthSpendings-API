using Application.Contracts;
using Application.Dto;
using Application.Interfaces;
using Application.Mappers;
using Application.Services;
using Domain;
using Microsoft.Extensions.Logging;

namespace Application.UseCases
{
    public interface IGetUserByIdUseCase
    {
        Task<CaseResult<AppUserDto?>> InvokeAsync();
    }

    public class GetUserByIdUseCase : IGetUserByIdUseCase
    {
        private IUnitOfWork _UnitOfWork { get; set; }
        private IUserService _UserService { get; set; }
        private readonly ILogger<GetUserByIdUseCase> _Logger;
        public GetUserByIdUseCase(IUnitOfWork unitOfWork, IUserService userService, ILogger<GetUserByIdUseCase> logger)
        {
            _UnitOfWork = unitOfWork;
            _UserService = userService;
            _Logger = logger;
        }

        public async Task<CaseResult<AppUserDto?>> InvokeAsync()
        {
            var result = new CaseResult<AppUserDto?>(null);
            result.Successful = true;

            try
            {
                int userId = _UserService.GetUserId();
                AppUser? user = await _UnitOfWork.UserRepository.GetUserById(userId);

                if (user == null)
                {
                    result.Successful = false;
                    result.ErrorMessage = "Invalid user.";
                    _Logger.LogWarning("User {UserId} not found", userId);
                    return result;
                }

                result.Data = user.ToDto();
                _Logger.LogInformation("User {UserId} retrieved", userId);
            }
            catch (Exception ex)
            {
                result.Successful = false;
                result.ErrorMessage = "Invalid user.";
                _Logger.LogError(ex, "Error getting user");
            }

            return result;
        }
    }
}
