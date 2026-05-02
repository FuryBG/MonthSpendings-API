using Application.Contracts;
using Application.Dto.SaltEdge;
using Application.Interfaces;
using Application.Mappers;
using Application.Services;
using Microsoft.Extensions.Logging;

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
        private readonly ILogger<GetConnectedSaltEdgeBanksByUserUseCase> _Logger;

        public GetConnectedSaltEdgeBanksByUserUseCase(IUnitOfWork unitOfWork, IUserService userService, ILogger<GetConnectedSaltEdgeBanksByUserUseCase> logger)
        {
            _UnitOfWork = unitOfWork;
            _UserService = userService;
            _Logger = logger;
        }

        public async Task<CaseResult<List<SaltEdgeConnectionDto>>> InvokeAsync(CancellationToken cancellationToken)
        {
            var result = new CaseResult<List<SaltEdgeConnectionDto>>() { Successful = true };
            int userId = 0;

            try
            {
                userId = _UserService.GetUserId();
                var connections = await _UnitOfWork.SaltEdgeConnectionRepository.GetByUserIdAsync(userId, cancellationToken);
                result.Data = connections.Select(c => c.ToDto()).ToList();

                _Logger.LogInformation("Retrieved {Count} connected SaltEdge banks for user {UserId}", result.Data!.Count, userId);
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "Error retrieving connected SaltEdge banks for user {UserId}", userId);
                result.Successful = false;
                result.ErrorMessage = "Something got wrong during loading Salt Edge connected banks. Please try again later.";
            }

            return result;
        }
    }
}
