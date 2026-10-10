using Application.Resources;
using Application.Contracts;
using Application.Dto;
using Application.Interfaces;
using Application.Localization;
using Application.Services;
using Domain;
using Microsoft.Extensions.Logging;

namespace Application.UseCases
{
    public interface IUpdateUserLanguageUseCase
    {
        Task<CaseResult<string>> InvokeAsync(UpdateLanguageDto dto);
    }

    public class UpdateUserLanguageUseCase : IUpdateUserLanguageUseCase
    {
        private IUnitOfWork _UnitOfWork { get; set; }
        private IUserService _UserService { get; set; }
        private readonly ILogger<UpdateUserLanguageUseCase> _Logger;

        public UpdateUserLanguageUseCase(IUnitOfWork unitOfWork, IUserService userService, ILogger<UpdateUserLanguageUseCase> logger)
        {
            _UnitOfWork = unitOfWork;
            _UserService = userService;
            _Logger = logger;
        }

        public async Task<CaseResult<string>> InvokeAsync(UpdateLanguageDto dto)
        {
            if (!SupportedLanguages.All.Contains(dto.Language))
            {
                _Logger.LogInformation("Unsupported language {Language} requested by user {UserId}", dto.Language, _UserService.GetUserId());
                return CaseResult<string>.Error(Messages.UserUnsupportedLanguage);
            }

            int userId = _UserService.GetUserId();
            AppUser? user = await _UnitOfWork.UserRepository.GetUserById(userId);

            if (user == null)
            {
                _Logger.LogWarning("User {UserId} not found when updating language", userId);
                return CaseResult<string>.Error(Messages.UserInvalid);
            }

            if (user.Language != dto.Language)
            {
                user.Language = dto.Language;
                await _UnitOfWork.CommitAsync();
                _Logger.LogInformation("Language set to {Language} for user {UserId}.", dto.Language, userId);
            }

            return CaseResult<string>.Success(dto.Language);
        }
    }
}
