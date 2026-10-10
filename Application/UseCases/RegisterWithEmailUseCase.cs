using Application.Resources;
using System.Globalization;
using Application.Contracts;
using Application.Dto;
using Application.Interfaces;
using Domain;
using Microsoft.Extensions.Logging;

namespace Application.UseCases
{
    public interface IRegisterWithEmailUseCase
    {
        Task<CaseResult<AuthResponseDto?>> InvokeAsync(RegisterDto dto);
    }

    public class RegisterWithEmailUseCase : IRegisterWithEmailUseCase
    {
        private readonly IUnitOfWork _UnitOfWork;
        private readonly ITokenService _TokenService;
        private readonly IPasswordService _PasswordService;
        private readonly ILogger<RegisterWithEmailUseCase> _Logger;

        public RegisterWithEmailUseCase(
            IUnitOfWork unitOfWork,
            ITokenService tokenService,
            IPasswordService passwordService,
            ILogger<RegisterWithEmailUseCase> logger)
        {
            _UnitOfWork = unitOfWork;
            _TokenService = tokenService;
            _PasswordService = passwordService;
            _Logger = logger;
        }

        public async Task<CaseResult<AuthResponseDto?>> InvokeAsync(RegisterDto dto)
        {
            var result = new CaseResult<AuthResponseDto?>();

            var existing = await _UnitOfWork.UserRepository.GetUserByEmail(dto.Email);
            if (existing != null)
            {
                _Logger.LogInformation("Email registration rejected: email already belongs to user {UserId}", existing.Id);
                return CaseResult<AuthResponseDto?>.Error(Messages.AuthEmailExists, ErrorType.Conflict);
            }

            var user = new AppUser
            {
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                PasswordHash = _PasswordService.Hash(dto.Password),
                // Request culture comes from the Accept-Language header (RequestLocalization middleware).
                Language = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName,
            };

            _UnitOfWork.UserRepository.AddUser(user);
            await _UnitOfWork.CommitAsync();

            var accessToken = _TokenService.CreateAccessToken(user);
            var refreshToken = await _TokenService.CreateRefreshTokenAsync(user.Id);

            result.Successful = true;
            result.Data = new AuthResponseDto(accessToken, refreshToken.Token);
            _Logger.LogInformation("User registered with email, user {UserId}", user.Id);

            return result;
        }
    }
}
