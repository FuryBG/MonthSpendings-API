using Application.Dto.SaltEdge;
using Domain.SaltEdge;

namespace Application.Mappers
{
    public static class SaltEdgeConnectionMapper
    {
        public static SaltEdgeConnectionDto ToDto(this SaltEdgeConnection connection)
        {
            return new SaltEdgeConnectionDto()
            {
                Id = connection.Id,
                ProviderName = connection.ProviderName,
                ImageUrl = connection.BankImgUrl,
                ValidTo = connection.ExpiresOn,
                Accounts = connection.Accounts.Select(a => a.ToDto()).ToList()
            };
        }
    }
}
