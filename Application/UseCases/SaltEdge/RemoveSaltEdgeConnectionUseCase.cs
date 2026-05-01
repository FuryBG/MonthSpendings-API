using Application.Contracts;
using Application.Interfaces;
using Application.Services;
using SaltEdge.Interfaces;
using System.Net;

namespace Application.UseCases.SaltEdge
{
    public interface IRemoveSaltEdgeConnectionUseCase
    {
        Task<CaseResult<bool>> InvokeAsync(int connectionDbId, CancellationToken cancellationToken);
    }

    public class RemoveSaltEdgeConnectionUseCase : IRemoveSaltEdgeConnectionUseCase
    {
        private readonly IUnitOfWork _UnitOfWork;
        private readonly IUserService _UserService;
        private readonly IConnectionsService _ConnectionsService;

        public RemoveSaltEdgeConnectionUseCase(IUnitOfWork unitOfWork, IUserService userService, IConnectionsService connectionsService)
        {
            _UnitOfWork = unitOfWork;
            _UserService = userService;
            _ConnectionsService = connectionsService;
        }

        public async Task<CaseResult<bool>> InvokeAsync(int connectionDbId, CancellationToken cancellationToken)
        {
            var result = new CaseResult<bool>() { Successful = true, Data = true };

            try
            {
                int userId = _UserService.GetUserId();
                var connections = await _UnitOfWork.SaltEdgeConnectionRepository.GetByUserIdAsync(userId, cancellationToken);
                var connection = connections.FirstOrDefault(c => c.Id == connectionDbId);

                if (connection == null)
                {
                    result.Successful = false;
                    result.Data = false;
                    result.ErrorMessage = "Cannot find Salt Edge connection for the current user.";
                    return result;
                }

                if (!string.IsNullOrWhiteSpace(connection.ConnectionId))
                {
                    var deleteResponse = await _ConnectionsService.DeleteAsync(connection.ConnectionId, cancellationToken);
                    
                    if (deleteResponse.StatusCode != HttpStatusCode.OK)
                    {
                        result.Successful = false;
                        result.Data = false;
                        result.ErrorMessage = deleteResponse.Error?.Message ?? "Cannot remove Salt Edge connection.";
                        return result;
                    }
                }

                await _UnitOfWork.SaltEdgeConnectionRepository.DeleteAsync(c => c.Id == connectionDbId && c.UserId == userId, cancellationToken);
                await _UnitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                result.Successful = false;
                result.Data = false;
                result.ErrorMessage = "Something got wrong during remove Salt Edge bank account. Please try again later.";
            }

            return result;
        }
    }
}
