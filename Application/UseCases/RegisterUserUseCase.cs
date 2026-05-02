using Application.Contracts;
using Application.Dto;
using Application.Interfaces;
using Domain;
using Microsoft.Extensions.Logging;

namespace Application.UseCases
{
    public interface IRegisterUserUseCase
    {
        Task<CaseResult<string?>> InvokeAsync(GoogleUserDto googleUserDto);
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

        public async Task<CaseResult<string?>> InvokeAsync(GoogleUserDto googleUserDto)
        {
            var result = new CaseResult<string?>();
            result.Successful = true;

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

                string jwtToken = _TokenService.CreateToken(user);
                result.Data = jwtToken;
                _Logger.LogInformation("User registered/logged in, token issued");
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "Error registering user with Google ID {GoogleId}", googleUserDto?.Id);
                result.Successful = false;
                result.ErrorMessage = "Something got wrong during login. Please try again later.";
            }
            return result;
        }
    }
}
