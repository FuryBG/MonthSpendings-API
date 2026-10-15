using Application.Resources;
using Application.Contracts;
using Application.Dto;
using Application.Interfaces;
using Application.Services;
using Domain;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.NotificationTransactions
{
    public interface IGetWalletIngestTokenUseCase
    {
        Task<CaseResult<WalletIngestTokenDto>> InvokeAsync();
    }

    public class GetWalletIngestTokenUseCase : IGetWalletIngestTokenUseCase
    {
        private readonly IUnitOfWork _UnitOfWork;
        private readonly IUserService _UserService;
        private readonly ITokenService _TokenService;
        private readonly ILogger<GetWalletIngestTokenUseCase> _Logger;

        public GetWalletIngestTokenUseCase(IUnitOfWork unitOfWork, IUserService userService, ITokenService tokenService, ILogger<GetWalletIngestTokenUseCase> logger)
        {
            _UnitOfWork = unitOfWork;
            _UserService = userService;
            _TokenService = tokenService;
            _Logger = logger;
        }

        public async Task<CaseResult<WalletIngestTokenDto>> InvokeAsync()
        {
            int userId = _UserService.GetUserId();
            AppUser? user = await _UnitOfWork.UserRepository.GetUserById(userId);
            if (user == null)
            {
                _Logger.LogWarning("Wallet ingest token not issued: user {UserId} behind a valid token not found", userId);
                return CaseResult<WalletIngestTokenDto>.Error(Messages.UserNotFound);
            }

            return CaseResult<WalletIngestTokenDto>.Success(new WalletIngestTokenDto(_TokenService.CreateWalletIngestToken(user)));
        }
    }
}
