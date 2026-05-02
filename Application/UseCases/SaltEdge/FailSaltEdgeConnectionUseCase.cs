using Application.Contracts;
using Application.Interfaces;
using Domain.SaltEdge.Enums;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.SaltEdge
{
    public interface IFailSaltEdgeConnectionUseCase
    {
        Task<CaseResult<bool>> InvokeAsync(Guid localSessionId, string? connectionId, string? errorClass, string? errorMessage, CancellationToken cancellationToken);
    }

    public class FailSaltEdgeConnectionUseCase : IFailSaltEdgeConnectionUseCase
    {
        private readonly IUnitOfWork _UnitOfWork;
        private readonly ILogger<FailSaltEdgeConnectionUseCase> _Logger;

        public FailSaltEdgeConnectionUseCase(IUnitOfWork unitOfWork, ILogger<FailSaltEdgeConnectionUseCase> logger)
        {
            _UnitOfWork = unitOfWork;
            _Logger = logger;
        }

        public async Task<CaseResult<bool>> InvokeAsync(Guid localSessionId, string? connectionId, string? errorClass, string? errorMessage, CancellationToken cancellationToken)
        {
            var result = new CaseResult<bool>() { Successful = true, Data = true };

            try
            {
                var connection = await _UnitOfWork.SaltEdgeConnectionRepository.GetByLocalSessionIdAsync(localSessionId, cancellationToken);
                
                if (connection == null)
                {
                    result.Successful = false;
                    result.Data = false;
                    result.ErrorMessage = "Can't find initiated Salt Edge bank connection. Please try again.";
                    return result;
                }

                connection.ConnectionId = connectionId ?? connection.ConnectionId;
                connection.State = SaltEdgeConnectionStatus.ConnectionFailed;
                connection.LastErrorClass = errorClass;
                connection.LastErrorMessage = errorMessage;

                await _UnitOfWork.SaltEdgeConnectionRepository.UpdateAsync(connection, cancellationToken);
                await _UnitOfWork.CommitAsync();

                _Logger.LogInformation("SaltEdge connection marked as failed");
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "Error recording SaltEdge connection failure");
                result.Successful = false;
                result.Data = false;
                result.ErrorMessage = "Something got wrong during failing Salt Edge bank connection. Please try again later.";
            }

            return result;
        }
    }
}
