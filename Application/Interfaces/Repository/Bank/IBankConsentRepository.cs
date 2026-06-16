using Domain.Bank;
using System.Linq.Expressions;

namespace Application.Interfaces.Repository.Bank
{
    public interface IBankConsentRepository
    {
        Task<BankConsent> CreateBankConsent(BankConsent bankConsent);
        Task<BankConsent?> GetBankConsentBySessionId(Guid sessionId);
        Task<List<BankConsent>> GetBankConsentByUserId(int userId);
        Task<List<BankConsent>> ClaimConsentsForManualSyncAsync(int userId, CancellationToken ct);
        Task<BankConsent> Update(BankConsent bankConsent);
        Task<int> Delete(Expression<Func<BankConsent, bool>> expression, CancellationToken cancellationToken);
        Task MarkConsentsAsSyncedAsync(List<int> consentIds, DateTime syncedAt, CancellationToken ct);
        Task UpdateSyncStartedAtAsync(List<int> consentIds, DateTime? value, CancellationToken ct);
        Task<List<BankConsent>> GetConsentsForSync(DateTime threshold, CancellationToken cancellationToken);
    }
}