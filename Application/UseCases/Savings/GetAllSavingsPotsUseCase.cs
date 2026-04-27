using Application.Contracts;
using Application.Dto.Savings;
using Application.Interfaces;
using Application.Mappers;
using Application.Services;

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
        public GetAllSavingsPotsUseCase(IUnitOfWork unitOfWork, IUserService userService)
        {
            _UnitOfWork = unitOfWork;
            _UserService = userService;
        }

        public async Task<CaseResult<List<SavingsPotDto>>> InvokeAsync()
        {
            var result = new CaseResult<List<SavingsPotDto>>();
            result.Successful = true;

            try
            {
                int userId = _UserService.GetUserId();
                var pots = await _UnitOfWork.SavingsPotRepository.GetAllForUser(userId);
                result.Data = pots.Select(p => p.ToDto()).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                result.Successful = false;
                result.ErrorMessage = "Something went wrong while fetching savings pots.";
            }

            return result;
        }
    }
}
