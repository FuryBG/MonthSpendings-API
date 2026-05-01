using Application.Contracts;
using Application.Dto.SaltEdge;
using Application.Interfaces;
using Application.Services;
using Domain;
using Domain.SaltEdge;
using Domain.SaltEdge.Enums;
using Microsoft.Extensions.Configuration;
using SaltEdge.Interfaces;
using SaltEdge.Models.Connections;
using SaltEdge.Models.Customers;
using System.Net;

namespace Application.UseCases.SaltEdge
{
    public interface IStartSaltEdgeConnectionUseCase
    {
        Task<CaseResult<SaltEdgeStartConnectionDto>> InvokeAsync(string providerCode, string providerName, string countryCode, string bankImageUrl, int consentDays, CancellationToken cancellationToken);
    }

    public class StartSaltEdgeConnectionUseCase : IStartSaltEdgeConnectionUseCase
    {
        private readonly IUnitOfWork _UnitOfWork;
        private readonly IUserService _UserService;
        private readonly ICustomersService _CustomersService;
        private readonly IConnectionsService _ConnectionsService;
        private readonly IConfiguration _Configuration;

        public StartSaltEdgeConnectionUseCase(
            IUnitOfWork unitOfWork,
            IUserService userService,
            ICustomersService customersService,
            IConnectionsService connectionsService,
            IConfiguration configuration)
        {
            _UnitOfWork = unitOfWork;
            _UserService = userService;
            _CustomersService = customersService;
            _ConnectionsService = connectionsService;
            _Configuration = configuration;
        }

        public async Task<CaseResult<SaltEdgeStartConnectionDto>> InvokeAsync(string providerCode, string providerName, string countryCode, string bankImageUrl, int consentDays, CancellationToken cancellationToken)
        {
            var result = new CaseResult<SaltEdgeStartConnectionDto>() { Successful = true };

            try
            {
                int userId = _UserService.GetUserId();

                if (userId == 0)
                {
                    result.Successful = false;
                    result.ErrorMessage = "Can't find your personal information. Please first login, to connect your bank.";
                    return result;
                }

                SaltEdgeCustomer customer = await EnsureCustomerAsync(userId, cancellationToken);
                Guid localSessionId = Guid.NewGuid();

                var connection = new SaltEdgeConnection()
                {
                    UserId = userId,
                    SaltEdgeCustomerId = customer.Id,
                    LocalSessionId = localSessionId,
                    State = SaltEdgeConnectionStatus.Initiated,
                    ExpiresOn = DateTime.UtcNow.AddDays(consentDays),
                    BankImgUrl = bankImageUrl,
                    ProviderName = providerName,
                    ProviderCode = providerCode,
                    CountryCode = countryCode,
                };

                await _UnitOfWork.SaltEdgeConnectionRepository.CreateAsync(connection, cancellationToken);
                await _UnitOfWork.CommitAsync();

                string returnTo = _Configuration["SaltEdge:ConnectReturnUrl"] ?? "monthspendings://(main)/ConnectSaltEdgePending";
                DateOnly fromDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30));

                var response = await _ConnectionsService.ConnectAsync(new ConnectConnectionRequest()
                {
                    Data = new ConnectConnectionRequestData()
                    {
                        CustomerId = customer.CustomerId,
                        Provider = new ProviderRequest() { Code = providerCode },
                        Consent = new ConsentRequest()
                        {
                            Scopes = ["accounts", "transactions", "holder_info"],
                            FromDate = fromDate,
                            PeriodDays = consentDays,
                        },
                        Attempt = new AttemptRequest()
                        {
                            ReturnTo = returnTo,
                            FetchScopes = ["accounts", "transactions", "holder_info"],
                            CustomFields = new Dictionary<string, string>()
                            {
                                ["local_session_id"] = localSessionId.ToString(),
                                ["user_id"] = userId.ToString(),
                            }
                        }
                    }
                }, cancellationToken);

                if (response.StatusCode != HttpStatusCode.OK || response.Data?.ConnectUrl == null)
                {
                    connection.State = SaltEdgeConnectionStatus.ConnectionFailed;
                    connection.LastErrorMessage = response.Error?.Message;
                    await _UnitOfWork.SaltEdgeConnectionRepository.UpdateAsync(connection, cancellationToken);
                    await _UnitOfWork.CommitAsync();

                    result.Successful = false;
                    result.ErrorMessage = response.Error?.Message ?? "Cannot create Salt Edge connection.";
                    return result;
                }

                if (string.IsNullOrWhiteSpace(response.Data.ConnectUrl))
                {
                    connection.State = SaltEdgeConnectionStatus.ConnectionFailed;
                    await _UnitOfWork.SaltEdgeConnectionRepository.UpdateAsync(connection, cancellationToken);
                    await _UnitOfWork.CommitAsync();

                    result.Successful = false;
                    result.ErrorMessage = response.Error?.Message ?? "Cannot create Salt Edge connection.";
                    return result;
                }

                result.Data = new SaltEdgeStartConnectionDto()
                {
                    ConnectUrl = response.Data.ConnectUrl,
                    LocalSessionId = localSessionId,
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                result.Successful = false;
                result.ErrorMessage = "Something got wrong during starting Salt Edge bank connection. Please try again later.";
            }

            return result;
        }

        private async Task<SaltEdgeCustomer> EnsureCustomerAsync(int userId, CancellationToken cancellationToken)
        {
            SaltEdgeCustomer? existingCustomer = await _UnitOfWork.SaltEdgeCustomerRepository.GetByUserIdAsync(userId, cancellationToken);
            if (existingCustomer != null)
            {
                return existingCustomer;
            }

            var response = await _CustomersService.CreateAsync(new CreateCustomerRequest()
            {
                Data = new CreateCustomerRequestData()
                {
                    Identifier = $"{userId}"
                }
            }, cancellationToken);

            if (response.StatusCode != HttpStatusCode.Created && response.StatusCode != HttpStatusCode.OK || string.IsNullOrWhiteSpace(response.Data?.CustomerId))
            {
                throw new InvalidOperationException(response.Error?.Message ?? "Cannot create Salt Edge customer.");
            }

            var customer = new SaltEdgeCustomer()
            {
                UserId = userId,
                CustomerId = response.Data.CustomerId,
                Identifier = response.Data.Identifier ?? userId.ToString(),
            };

            await _UnitOfWork.SaltEdgeCustomerRepository.CreateAsync(customer, cancellationToken);
            await _UnitOfWork.CommitAsync();
            return customer;
        }
    }
}
