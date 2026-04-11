using Application.Interfaces.Repository.Bank;
using Domain.Bank;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

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

        public async Task<List<BankConsent>> GetBankConsentByUserId(int userId)
        {
            return await _DbContext.BankConsent.Where(b => b.UserId == userId && b.State == Domain.Bank.Enums.BankAccountStatus.Connected).Include(bc => bc.Accounts).ToListAsync();
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

        public async Task<int> Delete(Expression<Func<BankConsent, bool>> expression, CancellationToken cancellationToken)
        {
            return await _DbContext.BankConsent.Where(expression).ExecuteDeleteAsync(cancellationToken);
        }

        public async Task<List<BankConsent>> GetConsentsForSync(DateTime threshold, CancellationToken cancellationToken)
        {
            var claimedConsents = await _DbContext.BankConsent
                .FromSql($@"
                        UPDATE ""BankConsent""
                        SET ""SyncStartedAt"" = NOW()
                        WHERE
                            ""LastSync"" <= {threshold}
                            AND ""ExpiresOn"" > NOW()
                            AND (""SyncStartedAt"" IS NULL OR ""SyncStartedAt"" < NOW() - INTERVAL '10 minutes')
                        RETURNING *
    ").ToListAsync(cancellationToken);

            foreach (var consent in claimedConsents)
            {
                await _DbContext.Entry(consent).Collection(c => c.Accounts).LoadAsync(cancellationToken);
            }

            return claimedConsents;
        }

        public async Task MarkConsentsAsSyncedAsync(List<int> consentIds, DateTime syncedAt, CancellationToken ct)
        {
            await _DbContext.BankConsent
                .Where(c => consentIds.Contains(c.Id))
                .ExecuteUpdateAsync(c => c.SetProperty(c => c.LastSync, _ => syncedAt)
                                        .SetProperty(c => c.SyncStartedAt, _ => null), ct);
        }
    }
}