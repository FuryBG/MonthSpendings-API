using Application.Contracts;
using Application.Dto;
using Application.Interfaces;
using Application.Mappers;
using Application.Services;
using Domain;

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
        public GetUserByIdUseCase(IUnitOfWork unitOfWork, IUserService userService)
        {
            _UnitOfWork = unitOfWork;
            _UserService = userService;
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
                    //todo log
                    Console.WriteLine($"Can't find user with id {userId}.");
                    return result;
                }

                result.Data = user.ToDto();

            }
            catch (Exception ex)
            {
                result.Successful = false;
                result.ErrorMessage = "Invalid user.";
                //todo log
                Console.WriteLine(ex.Message);
            }

            return result;
        }
    }
}
