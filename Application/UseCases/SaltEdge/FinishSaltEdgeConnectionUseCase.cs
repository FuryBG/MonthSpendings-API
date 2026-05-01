using Application.Contracts;
using Application.Interfaces;
using Domain.SaltEdge;
using Domain.SaltEdge.Enums;
using SaltEdge.Interfaces;
using SaltEdge.Models.Accounts;
using System.Net;

namespace Application.UseCases.SaltEdge
{
    public interface IFinishSaltEdgeConnectionUseCase
    {
        Task<CaseResult<bool>> InvokeAsync(string connectionId, Guid localSessionId, string? stage, CancellationToken cancellationToken);
    }

    public class FinishSaltEdgeConnectionUseCase : IFinishSaltEdgeConnectionUseCase
    {
        private readonly IUnitOfWork _UnitOfWork;
        private readonly IConnectionsService _ConnectionsService;
        private readonly IAccountsService _AccountsService;

        public FinishSaltEdgeConnectionUseCase(IUnitOfWork unitOfWork, IConnectionsService connectionsService, IAccountsService accountsService)
        {
            _UnitOfWork = unitOfWork;
            _ConnectionsService = connectionsService;
            _AccountsService = accountsService;
        }

        public async Task<CaseResult<bool>> InvokeAsync(string connectionId, Guid localSessionId, string? stage, CancellationToken cancellationToken)
        {
            var result = new CaseResult<bool>() { Successful = true, Data = true };

            try
            {
                SaltEdgeConnection? connection = await _UnitOfWork.SaltEdgeConnectionRepository.GetByLocalSessionIdAsync(localSessionId, cancellationToken);
                
                if (connection == null)
                {
                    result.Successful = false;
                    result.Data = false;
                    result.ErrorMessage = "Can't find initiated Salt Edge bank connection. Please try again.";
                    return result;
                }

                var connectionResponse = await _ConnectionsService.GetAsync(connectionId, cancellationToken);
                if (connectionResponse.StatusCode != HttpStatusCode.OK || connectionResponse.Data == null)
                {
                    result.Successful = false;
                    result.Data = false;
                    result.ErrorMessage = connectionResponse.Error?.Message ?? "Cannot load Salt Edge connection.";
                    return result;
                }

                var accountsResponse = await _AccountsService.GetAsync(connectionId, cancellationToken);
                if (accountsResponse.StatusCode != HttpStatusCode.OK || accountsResponse.Data == null)
                {
                    result.Successful = false;
                    result.Data = false;
                    result.ErrorMessage = accountsResponse.Error?.Message ?? "Cannot load Salt Edge accounts.";
                    return result;
                }

                connection.ConnectionId = connectionId;
                connection.ProviderName = connectionResponse.Data.ProviderName ?? connection.ProviderName;
                connection.ProviderCode = connectionResponse.Data.ProviderCode ?? connection.ProviderCode;
                connection.CountryCode = connectionResponse.Data.CountryCode ?? connection.CountryCode;
                connection.ExpiresOn = connectionResponse.Data.Consent?.ExpiresAt ?? connection.ExpiresOn;
                connection.LastStage = stage ?? connection.LastStage;
                connection.State = string.Equals(connectionResponse.Data.Status, "active", StringComparison.OrdinalIgnoreCase)
                    ? SaltEdgeConnectionStatus.Connected
                    : SaltEdgeConnectionStatus.Initiated;

                await _UnitOfWork.SaltEdgeAccountRepository.DeleteByConnectionDbIdAsync(connection.Id, cancellationToken);

                List<SaltEdgeAccount> accounts = accountsResponse.Data
                    .Where(a => !string.IsNullOrWhiteSpace(a.Id))
                    .Select(a => new SaltEdgeAccount()
                    {
                        AccountId = a.Id!,
                        ConnectionDbId = connection.Id,
                        Currency = a.CurrencyCode ?? "not available",
                        HolderName = a.Extra?.HolderName ?? a.Name ?? "not available",
                        Iban = a.Extra?.Iban ?? "not available",
                    })
                    .ToList();

                if (accounts.Count > 0)
                {
                    await _UnitOfWork.SaltEdgeAccountRepository.AddRangeAsync(accounts, cancellationToken);
                }

                await _UnitOfWork.SaltEdgeConnectionRepository.UpdateAsync(connection, cancellationToken);
                await _UnitOfWork.CommitAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                result.Successful = false;
                result.Data = false;
                result.ErrorMessage = "Something got wrong during finish Salt Edge bank connection. Please try again later.";
            }

            return result;
        }
    }
}
