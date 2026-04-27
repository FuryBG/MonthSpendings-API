using Application.Contracts;
using Application.Dto.Savings;
using Application.Interfaces;
using Application.Mappers;
using Application.Services;
using Domain;

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
        public CreateSavingsPotUseCase(IUnitOfWork unitOfWork, IUserService userService)
        {
            _UnitOfWork = unitOfWork;
            _UserService = userService;
        }

        public async Task<CaseResult<SavingsPotDto?>> InvokeAsync(SavingsPotDto dto)
        {
            var result = new CaseResult<SavingsPotDto?>();
            result.Successful = true;

            try
            {
                int userId = _UserService.GetUserId();
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
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                result.Successful = false;
                result.ErrorMessage = "Something went wrong while creating the savings pot.";
            }

            return result;
        }
    }
}
