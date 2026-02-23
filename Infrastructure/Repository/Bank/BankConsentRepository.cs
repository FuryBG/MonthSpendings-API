using Application.Interfaces.Repository.Bank;
using Domain.Bank;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository.Bank
{
    public class BankConsentRepository : IBankConsentRepository
    {
        private AppDbContext _DbContext { get; set; }
        public BankConsentRepository(AppDbContext dbContext)
        {
            _DbContext = dbContext;
        }

        public async Task<BankConsent?> GetBankConsentBySessionId(Guid sessionId)
        {
            return await _DbContext.BankConsent.FirstOrDefaultAsync(b => b.SessionId == sessionId);
        }

        public async Task<BankConsent> CreateBankConsent(BankConsent bankConsent)
        {
            _DbContext.BankConsent.Add(bankConsent);
            return bankConsent;
        }

        public async Task<BankConsent> Update(BankConsent bankConsent)
        {
            _DbContext.BankConsent.Update(bankConsent);
            return bankConsent;
        }
    }
}
