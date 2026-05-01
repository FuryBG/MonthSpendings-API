using Application.Contracts;
using Application.Dto.SaltEdge;
using Application.Mappers;
using SaltEdge.Interfaces;
using SaltEdge.Models.Providers;

namespace Application.UseCases.SaltEdge
{
    public interface IGetSaltEdgeBanksUseCase
    {
        Task<CaseResult<List<SaltEdgeProviderDto>>> InvokeAsync(string? bankName, CancellationToken cancellationToken);
    }

    public class GetSaltEdgeBanksUseCase : IGetSaltEdgeBanksUseCase
    {
        private readonly IProvidersService _ProvidersService;

        public GetSaltEdgeBanksUseCase(IProvidersService providersService)
        {
            _ProvidersService = providersService;
        }

        public async Task<CaseResult<List<SaltEdgeProviderDto>>> InvokeAsync(string? bankName, CancellationToken cancellationToken)
        {
            var result = new CaseResult<List<SaltEdgeProviderDto>>() { Successful = true };

            try
            {
                var response = await _ProvidersService.GetAsync(includeSandboxes: true, cancellationToken);

                if (response.Data == null || response.Error != null)
                {
                    result.Successful = false;
                    result.ErrorMessage = response.Error?.Message ?? "Cannot load Salt Edge providers.";
                    return result;
                }

                IEnumerable<Provider> providers = response.Data;

                if (!string.IsNullOrWhiteSpace(bankName))
                {
                    providers = providers.Where(p => p.Name != null && p.Name.Contains(bankName, StringComparison.OrdinalIgnoreCase));
                }

                result.Data = providers
                    .Where(p => string.Equals(p.Status, "active", StringComparison.OrdinalIgnoreCase))
                    .Select(p => p.ToDto())
                    .ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                result.Successful = false;
                result.ErrorMessage = "Something got wrong during getting Salt Edge banks. Please try again later.";
            }

            return result;
        }
    }
}
