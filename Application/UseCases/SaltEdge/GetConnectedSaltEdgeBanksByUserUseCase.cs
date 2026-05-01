using Application.Contracts;
using Application.Dto.SaltEdge;
using Application.Interfaces;
using Application.Mappers;
using Application.Services;

namespace Application.UseCases.SaltEdge
{
    public interface IGetConnectedSaltEdgeBanksByUserUseCase
    {
        Task<CaseResult<List<SaltEdgeConnectionDto>>> InvokeAsync(CancellationToken cancellationToken);
    }

    public class GetConnectedSaltEdgeBanksByUserUseCase : IGetConnectedSaltEdgeBanksByUserUseCase
    {
        private readonly IUnitOfWork _UnitOfWork;
        private readonly IUserService _UserService;

        public GetConnectedSaltEdgeBanksByUserUseCase(IUnitOfWork unitOfWork, IUserService userService)
        {
            _UnitOfWork = unitOfWork;
            _UserService = userService;
        }

        public async Task<CaseResult<List<SaltEdgeConnectionDto>>> InvokeAsync(CancellationToken cancellationToken)
        {
            var result = new CaseResult<List<SaltEdgeConnectionDto>>() { Successful = true };

            try
            {
                int userId = _UserService.GetUserId();
                var connections = await _UnitOfWork.SaltEdgeConnectionRepository.GetByUserIdAsync(userId, cancellationToken);
                result.Data = connections.Select(c => c.ToDto()).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                result.Successful = false;
                result.ErrorMessage = "Something got wrong during loading Salt Edge connected banks. Please try again later.";
            }

            return result;
        }
    }
}
