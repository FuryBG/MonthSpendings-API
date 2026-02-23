using Domain.Bank;

namespace Application.Interfaces.Repository.Bank
{
    public interface IBankConsentRepository
    {
        Task<BankConsent> CreateBankConsent(BankConsent bankConsent);
        Task<BankConsent?> GetBankConsentBySessionId(Guid sessionId);
        Task<BankConsent> Update(BankConsent bankConsent);
    }
}