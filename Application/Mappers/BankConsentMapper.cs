using Application.Dto.Bank;
using Domain.Bank;

namespace Application.Mappers
{
    public static class BankConsentMapper
    {
        public static BankConsentDto ToDto(this BankConsent bankConsent)
        {
            return new BankConsentDto()
            {
                Id = bankConsent.Id,
                BankName = bankConsent.BankName,
                ImageUrl = bankConsent.BankImgUrl,
                ValidTo = bankConsent.ExpiresOn,
                BankAccounts = bankConsent.Accounts.Select(acc => acc.ToDto()).ToList()
            };
        }
    }
}
