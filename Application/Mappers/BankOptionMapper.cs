using Application.Dto.Bank;
using EnableBanking.Models.General;

namespace Application.Mappers
{
    public static class BankOptionMapper
    {
        public static BankOptionDto ToDto(this Aspsp aspsp)
        {
            return new BankOptionDto()
            {
                Name = aspsp.Name ?? string.Empty,
                Country = aspsp.Country ?? string.Empty,
                Logo = aspsp.Logo ?? string.Empty,
                Bic = aspsp.Bic ?? string.Empty,
                MaximumConsentValidity = aspsp.MaximumConsentValidity ?? 0
            };
        }
    }
}
