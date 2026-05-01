using Application.Dto.SaltEdge;
using SaltEdge.Models.Providers;

namespace Application.Mappers
{
    public static class SaltEdgeProviderMapper
    {
        public static SaltEdgeProviderDto ToDto(this Provider provider)
        {
            return new SaltEdgeProviderDto()
            {
                Code = provider.Code ?? string.Empty,
                Name = provider.Name ?? string.Empty,
                CountryCode = provider.CountryCode ?? string.Empty,
                LogoUrl = provider.LogoUrl,
                Mode = provider.Mode,
                Regulated = provider.Regulated
            };
        }
    }
}
