using Application.Contracts;
using Application.Dto.Savings;
using Application.Interfaces;
using Application.Mappers;
using Application.Services;
using Domain;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.Savings
{
    public interface ICreateSavingsPotUseCase
    {
        Task<CaseResult<SavingsPotDto?>> InvokeAsync(SavingsPotDto dto);
    }

    public class CreateSavingsPotUseCase : ICreateSavingsPotUseCase
    {
        private IUnitOfWork _UnitOfWork { get; set; }
        private IUserService _UserService { get; set; }
        private readonly ILogger<CreateSavingsPotUseCase> _Logger;
        public CreateSavingsPotUseCase(IUnitOfWork unitOfWork, IUserService userService, ILogger<CreateSavingsPotUseCase> logger)
        {
            _UnitOfWork = unitOfWork;
            _UserService = userService;
            _Logger = logger;
        }

        public async Task<CaseResult<SavingsPotDto?>> InvokeAsync(SavingsPotDto dto)
        {
            var result = new CaseResult<SavingsPotDto?>();
            result.Successful = true;
            int userId = 0;

            try
            {
                userId = _UserService.GetUserId();
                AppUser? user = await _UnitOfWork.UserRepository.GetUserById(userId);

                if (user == null)
                {
                    result.Successful = false;
                    result.ErrorMessage = "User not found.";
                    return result;
                }

                var pot = new SavingsPot
                {
                    Name = dto.Name,
                    CurrencyId = dto.Currency.Id,
                    Currency = dto.Currency.ToEntity(),
                    CreatedByUserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    Users = new List<AppUser> { user },
                };

                var created = _UnitOfWork.SavingsPotRepository.Create(pot);
                await _UnitOfWork.CommitAsync();
                result.Data = created.ToDto();
                _Logger.LogInformation("Savings pot {PotId} created by user {UserId}", created.Id, userId);
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "Error creating savings pot for user {UserId}", userId);
                result.Successful = false;
                result.ErrorMessage = "Something went wrong while creating the savings pot.";
            }

            return result;
        }
    }
}
