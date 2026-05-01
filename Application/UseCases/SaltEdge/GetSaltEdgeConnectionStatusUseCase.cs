using Application.Contracts;
using Application.Dto.SaltEdge;
using Application.Interfaces;
using Application.Services;
using Domain.SaltEdge;

namespace Application.UseCases.SaltEdge
{
    public interface IGetSaltEdgeConnectionStatusUseCase
    {
        Task<CaseResult<SaltEdgeConnectionStatusDto>> InvokeAsync(Guid localSessionId, CancellationToken cancellationToken);
    }

    public class GetSaltEdgeConnectionStatusUseCase : IGetSaltEdgeConnectionStatusUseCase
    {
        private readonly IUnitOfWork _UnitOfWork;
        private readonly IUserService _UserService;

        public GetSaltEdgeConnectionStatusUseCase(IUnitOfWork unitOfWork, IUserService userService)
        {
            _UnitOfWork = unitOfWork;
            _UserService = userService;
        }

        public async Task<CaseResult<SaltEdgeConnectionStatusDto>> InvokeAsync(Guid localSessionId, CancellationToken cancellationToken)
        {
            var result = new CaseResult<SaltEdgeConnectionStatusDto>() { Successful = true };

            try
            {
                int userId = _UserService.GetUserId();
                SaltEdgeConnection? connection = await _UnitOfWork.SaltEdgeConnectionRepository.GetByLocalSessionIdAsync(localSessionId, cancellationToken);

                if (connection == null || connection.UserId != userId)
                {
                    result.Successful = false;
                    result.ErrorMessage = "Can't find initiated Salt Edge bank connection. Please try again.";
                    return result;
                }

                result.Data = new SaltEdgeConnectionStatusDto()
                {
                    LocalSessionId = connection.LocalSessionId,
                    State = connection.State.ToString(),
                    ProviderName = connection.ProviderName,
                    ErrorMessage = connection.LastErrorMessage,
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                result.Successful = false;
                result.ErrorMessage = "Something got wrong during loading Salt Edge connection status. Please try again later.";
            }

            return result;
        }
    }
}
