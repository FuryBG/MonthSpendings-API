using Application.Contracts;
using Application.Dto;
using Application.Interfaces;
using Domain;
using Microsoft.Extensions.Logging;


namespace Application.UseCases
{
    public interface IRegisterUserUseCase
    {
        Task<CaseResult<AuthResponseDto?>> InvokeAsync(GoogleUserDto googleUserDto);
    }

    public class RegisterUserUseCase : IRegisterUserUseCase
    {
        private ITokenService _TokenService { get; set; }
        private IUnitOfWork _UnitOfWork { get; set; }
        private readonly ILogger<RegisterUserUseCase> _Logger;

        public RegisterUserUseCase(ITokenService tokenService, IUnitOfWork unitOfWork, ILogger<RegisterUserUseCase> logger)
        {
            _TokenService = tokenService;
            _UnitOfWork = unitOfWork;
            _Logger = logger;
        }

        public async Task<CaseResult<AuthResponseDto?>> InvokeAsync(GoogleUserDto googleUserDto)
        {
            var result = new CaseResult<AuthResponseDto?>();
            result.Successful = true;

            if (googleUserDto == null || string.IsNullOrEmpty(googleUserDto.Id))
            {
                result.Successful = false;
                result.ErrorMessage = "Invalid Google credentials.";
                return result;
            }

            try
            {
                AppUser? user = await _UnitOfWork.UserRepository.GetUserByGoogleId(googleUserDto.Id);

                if (user == null)
                {
                    user = new AppUser()
                    {
                        FirstName = googleUserDto.FirstName,
                        LastName = googleUserDto.LastName,
                        Email = googleUserDto.Email,
                        GoogleId = googleUserDto.Id,
                        GooglePhotoAddress = googleUserDto.PhotoAddress,
                        NotificationToken = googleUserDto.NotificationToken
                    };
                    _UnitOfWork.UserRepository.AddUser(user);
                    await _UnitOfWork.CommitAsync();
                }

                var accessToken = _TokenService.CreateAccessToken(user);
                var refreshToken = await _TokenService.CreateRefreshTokenAsync(user.Id);

                result.Data = new AuthResponseDto(accessToken, refreshToken.Token);
                _Logger.LogInformation("User registered/logged in via Google, tokens issued for user {UserId}", user.Id);
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "Error registering user with Google ID {GoogleId}", googleUserDto?.Id);
                result.Successful = false;
                result.ErrorMessage = "Something went wrong during login. Please try again later.";
            }
            return result;
        }
    }
}
