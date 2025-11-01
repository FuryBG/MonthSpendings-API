using Application.Interfaces;

namespace Application
{
    public class RegisterUserUseCase
    {
        private ITokenService _TokenService { get; set; }
        public RegisterUserUseCase(ITokenService tokenService)
        {
            _TokenService = tokenService;
        }

        public async Task InvokeAsync(GoogleUserDto googleUserDto)
        {

        }
    }
}
