using Application.Dto.Budget;
using Domain;

namespace Application.Mappers
{
    public static class CurrencyMapper
    {
        public static CurrencyDto ToDto(this Currency currency)
        {
            return new CurrencyDto()
            {
                Id = currency.Id,
                Code = currency.Code,
                Name = currency.Name,
                Symbol = currency.Symbol,
            };
        }
        public static Currency ToEntity(this CurrencyDto dto)
        {
            return new Currency()
            {
                Id = dto.Id,
                Code = dto.Code,
                Name = dto.Name,
                Symbol = dto.Symbol,
            };
        }
    }
}
