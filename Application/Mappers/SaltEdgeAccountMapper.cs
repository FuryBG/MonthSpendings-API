using Application.Dto.SaltEdge;
using Domain.SaltEdge;

namespace Application.Mappers
{
    public static class SaltEdgeAccountMapper
    {
        public static SaltEdgeAccountDto ToDto(this SaltEdgeAccount account)
        {
            return new SaltEdgeAccountDto()
            {
                Id = account.Id,
                Iban = account.Iban,
            };
        }
    }
}
