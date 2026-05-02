using Application.Contracts;
using Application.Dto.Savings;
using Application.Interfaces;
using Application.Mappers;
using Application.Services;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.Savings
{
    public interface IGetAllSavingsPotsUseCase
    {
        Task<CaseResult<List<SavingsPotDto>>> InvokeAsync();
    }

    public class GetAllSavingsPotsUseCase : IGetAllSavingsPotsUseCase
    {
        private IUnitOfWork _UnitOfWork { get; set; }
        private IUserService _UserService { get; set; }
        private readonly ILogger<GetAllSavingsPotsUseCase> _Logger;
        public GetAllSavingsPotsUseCase(IUnitOfWork unitOfWork, IUserService userService, ILogger<GetAllSavingsPotsUseCase> logger)
        {
            _UnitOfWork = unitOfWork;
            _UserService = userService;
            _Logger = logger;
        }

        public async Task<CaseResult<List<SavingsPotDto>>> InvokeAsync()
        {
            var result = new CaseResult<List<SavingsPotDto>>();
            result.Successful = true;
            int userId = 0;

            try
            {
                userId = _UserService.GetUserId();
                var pots = await _UnitOfWork.SavingsPotRepository.GetAllForUser(userId);
                result.Data = pots.Select(p => p.ToDto()).ToList();

                _Logger.LogInformation("Retrieved {Count} savings pots for user {UserId}", result.Data!.Count, userId);
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "Error retrieving savings pots for user {UserId}", userId);
                result.Successful = false;
                result.ErrorMessage = "Something went wrong while fetching savings pots.";
            }

            return result;
        }
    }
}
