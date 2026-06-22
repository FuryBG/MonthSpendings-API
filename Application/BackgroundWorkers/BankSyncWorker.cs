using Application.Interfaces;
using Domain;
using Domain.Bank;
using Domain.Bank.Enums;
using EnableBanking.Interfaces;
using EnableBanking.Models;
using EnableBanking.Models.Accounts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Application.BackgroundWorkers
{
    public interface IBankSyncWorker
    {
        Task SyncBankAccountsAsync(CancellationToken ct);
        Task SyncBankAccountsByUserAsync(int userId, CancellationToken ct);
    }

    public sealed class BankSyncWorker : IBankSyncWorker
    {
        private readonly IAccountsService _AccountService;
        private readonly IUnitOfWork _UnitOfWork;
        private readonly ILogger<BankSyncWorker> _Logger;
        private readonly IConfiguration _Configuration;

        public BankSyncWorker(IAccountsService accountService, IUnitOfWork unitOfWork, IConfiguration configuration, ILogger<BankSyncWorker> logger)
        {
            _AccountService = accountService;
            _UnitOfWork = unitOfWork;
            _Configuration = configuration;
            _Logger = logger;
        }

        public async Task SyncBankAccountsAsync(CancellationToken cancellationToken)
        {
            bool intervalSet = int.TryParse(_Configuration.GetSection("EnableBanking:TransactionSyncIntervalInMinutes").Value, out int updateInterval);

            if (!intervalSet)
            {
                throw new ApplicationException("EnableBanking:TransactionSyncIntervalInMinutes is not set.");
            }

            var threshold = DateTime.UtcNow.AddMinutes(-updateInterval);
            List<BankConsent> bankConsents = await _UnitOfWork.BankConsentRepository.GetConsentsForSync(threshold, cancellationToken);

            if (bankConsents.Count == 0) return;

            List<int> consentIds = bankConsents.Select(c => c.Id).ToList();

            try
            {
                foreach (BankConsent bankConsent in bankConsents)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    List<TransactionCategoryRule> rules = await _UnitOfWork.TransactionCategoryRuleRepository
                        .GetByUserIdAsync(bankConsent.UserId, cancellationToken);

                    foreach (BankAccount account in bankConsent.Accounts)
                    {
                        await SyncAccountAsync(bankConsent, account, rules, cancellationToken);
                    }

                    await _UnitOfWork.BankConsentRepository.MarkConsentsAsSyncedAsync([bankConsent.Id], DateTime.UtcNow, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, ex.Message);
                await _UnitOfWork.RollbackTransactionAsync();
                await _UnitOfWork.BankConsentRepository.UpdateSyncStartedAtAsync(consentIds, null, cancellationToken);
            }
        }

        public async Task SyncBankAccountsByUserAsync(int userId, CancellationToken cancellationToken)
        {
            List<BankConsent> bankConsents = await _UnitOfWork.BankConsentRepository.ClaimConsentsForManualSyncAsync(userId, cancellationToken);

            if (bankConsents.Count == 0) return;

            List<int> consentIds = bankConsents.Select(c => c.Id).ToList();

            try
            {
                foreach (BankConsent bankConsent in bankConsents)
                {
                    if (cancellationToken.IsCancellationRequested) break;

                    List<TransactionCategoryRule> rules = await _UnitOfWork.TransactionCategoryRuleRepository
                        .GetByUserIdAsync(bankConsent.UserId, cancellationToken);

                    foreach (BankAccount account in bankConsent.Accounts)
                    {
                        await SyncAccountAsync(bankConsent, account, rules, cancellationToken);
                    }

                    await _UnitOfWork.BankConsentRepository.MarkConsentsAsSyncedAsync([bankConsent.Id], DateTime.UtcNow, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, ex.Message);
                await _UnitOfWork.RollbackTransactionAsync();
                await _UnitOfWork.BankConsentRepository.UpdateSyncStartedAtAsync(consentIds, null, cancellationToken);
            }
        }

        private async Task SyncAccountAsync(BankConsent bankConsent, BankAccount account, List<TransactionCategoryRule> rules, CancellationToken cancellationToken)
        {
            string? continuationKey = null;
            do
            {
                GetTransactionsRequest request = new GetTransactionsRequest
                {
                    AccountId = account.AccountUuid,
                    TransactionStatus = TransactionStatus.BOOK.ToString(),
                    DateFrom = bankConsent.LastSync.AddDays(-5),
                    ContinuationKey = continuationKey,
                };

                ApiResponse<GetTransactionsResponse> response = await _AccountService.GetTransactionsAsync(request, cancellationToken);

                if (response.StatusCode != HttpStatusCode.OK || response.Error != null || response.Data == null)
                {
                    _Logger.LogCritical("Cannot get transactions for account {AccountId}. Error: {Error}", account.Id, response.Error?.Message);
                    return;
                }

                if (response.Data.Transactions != null)
                {
                    await _UnitOfWork.BeginTransactionAsync();

                    Transaction[] expenseTransactions = response.Data.Transactions
                        .Where(t => t.CreditDebitIndicator == "DBIT")
                        .ToArray();

                    List<(Transaction Transaction, decimal Amount, DateTime BookingDate, TransactionStatus Status, string TransactionId, bool Determined)> parsedTransactions = [];

                    foreach (Transaction transaction in response.Data.Transactions)
                    {
                        bool isAmountParsed = decimal.TryParse(transaction.TransactionAmount?.Amount, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal amount);
                        bool bookingDateParsed = DateTime.TryParse(transaction.BookingDate, out DateTime bookingDate);
                        bool statusParsed = Enum.TryParse(transaction.Status, ignoreCase: true, out TransactionStatus transactionStatus);

                        if (!isAmountParsed || !bookingDateParsed || !statusParsed)
                        {
                            _Logger.LogWarning("Cannot parse transaction fields — amount: {Amount}, bookingDate: {Date}, status: {Status}",
                                transaction.TransactionAmount?.Amount, transaction.BookingDate, transaction.Status);
                            continue;
                        }

                        // Fingerprint stays based on the unsigned amount so it matches transactions
                        // already stored before signed amounts were introduced.
                        string transactionId = CalculateTransactionId(account.Id, transaction, amount, bookingDate);

                        var (isIncoming, determined) = DetermineDirection(transaction, account);
                        decimal signedAmount = isIncoming ? amount : -amount;
                        parsedTransactions.Add((transaction, signedAmount, bookingDate, transactionStatus, transactionId, determined));
                    }

                    HashSet<string> existingTransactionIds = await _UnitOfWork.BankTransactionRepository
                        .GetExistingTransactionIdsAsync(parsedTransactions.Select(p => p.TransactionId), cancellationToken);

                    foreach (var (transaction, amount, bookingDate, transactionStatus, transactionId, determined) in parsedTransactions)
                    {
                        if (!existingTransactionIds.Add(transactionId))
                        {
                            _Logger.LogDebug("Skipping already-synced transaction {TransactionId}", transactionId);
                            continue;
                        }

                        string? description = transaction.RemittanceInformation?.FirstOrDefault()
                            ?? transaction.TransactionInformation
                            ?? transaction.Note;

                        BankTransaction bankTransaction = new BankTransaction
                        {
                            Currency = account.Currency,
                            Amount = amount,
                            MerchantCode = transaction.MerchantCategoryCode,
                            CreditorName = transaction.Creditor?.Name,
                            Description = description,
                            BookingDate = DateTime.SpecifyKind(bookingDate, DateTimeKind.Utc),
                            TransactionId = transactionId,
                            Status = transactionStatus,
                            BankAccountId = account.Id,
                            Determined = determined,
                        };

                        await _UnitOfWork.BankTransactionRepository.AddTransaction(bankTransaction, cancellationToken);
                    }

                    await _UnitOfWork.CommitTransactionAsync();

                    await ApplyRulesAsync(expenseTransactions, account, bankConsent.UserId, rules, cancellationToken);
                }

                continuationKey = response.Data.ContinuationKey;
            } while (!string.IsNullOrEmpty(continuationKey));
        }

        private async Task ApplyRulesAsync(Transaction[] transactions, BankAccount account, int userId, List<TransactionCategoryRule> rules, CancellationToken cancellationToken)
        {
            if (rules.Count == 0)
            {
                return;
            }

            foreach (Transaction transaction in transactions)
            {
                string searchText = string.Join(" ", new[] { transaction.Creditor?.Name, transaction.RemittanceInformation?.FirstOrDefault() ?? transaction.TransactionInformation ?? transaction.Note }
                    .Where(s => !string.IsNullOrEmpty(s)));

                if (string.IsNullOrEmpty(searchText))
                {
                    continue;
                }

                TransactionCategoryRule? matchedRule = rules.FirstOrDefault(r =>
                    searchText.Contains(r.Keyword, StringComparison.OrdinalIgnoreCase));

                if (matchedRule == null)
                {
                    continue;
                }

                bool isAmountParsed = decimal.TryParse(transaction.TransactionAmount?.Amount, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal amount);
                bool bookingDateParsed = DateTime.TryParse(transaction.BookingDate, out DateTime bookingDate);

                if (!isAmountParsed || !bookingDateParsed)
                {
                    continue;
                }

                string transactionId = CalculateTransactionId(account.Id, transaction, amount, bookingDate);

                BankTransaction? saved = await _UnitOfWork.BankTransactionRepository
                    .GetTransactionByTransactionId(transactionId, cancellationToken);

                if (saved == null || saved.Categorized)
                {
                    continue;
                }

                BudgetCategory? category = await _UnitOfWork.BudgetCategoryRepository
                    .GetBudgetCategoryById(matchedRule.CategoryId, userId);

                if (category == null)
                {
                    continue;
                }

                BudgetPeriod? period = category.Budget.BudgetPeriods.FirstOrDefault();
                if (period == null)
                {
                    continue;
                }

                await _UnitOfWork.BeginTransactionAsync();

                Spending spending = _UnitOfWork.CategorySpendingsRepository.AddSpending(new Spending
                {
                    Amount = saved.Amount,
                    BankTransactionId = saved.Id,
                    Date = saved.BookingDate,
                    BudgetCategoryId = category.Id,
                    BudgetPeriodId = period.Id,
                    CreatedByUserId = userId,
                });

                await _UnitOfWork.CommitAsync();
                await _UnitOfWork.BankTransactionRepository.CategorizeAsync([saved.Id], spending.Id, cancellationToken);
                await _UnitOfWork.CommitTransactionAsync();

                _Logger.LogInformation("Auto-categorized transaction {TransactionId} via rule {RuleId}", saved.Id, matchedRule.Id);
            }
        }

        // Returns (isIncoming, determined). determined=false when the bank reports our own
        // IBAN as the creditor with no counterparty info — some ASPSPs do this for card
        // charges (e.g. Google Pay via DSK), making it impossible to tell automatically
        // whether the transaction is income or an expense.
        private static (bool isIncoming, bool determined) DetermineDirection(Transaction transaction, BankAccount account)
        {
            string? creditorIban = transaction.CreditorAccount?.Iban;
            if (!string.IsNullOrEmpty(creditorIban))
            {
                bool userIsCreditor = string.Equals(creditorIban, account.Iban, StringComparison.OrdinalIgnoreCase);
                if (userIsCreditor)
                {
                    bool hasCounterparty = !string.IsNullOrEmpty(transaction.DebtorAccount?.Iban)
                                          || transaction.Debtor != null;
                    return (isIncoming: true, determined: hasCounterparty);
                }
                return (isIncoming: false, determined: true);
            }

            string? debtorIban = transaction.DebtorAccount?.Iban;
            if (!string.IsNullOrEmpty(debtorIban))
            {
                bool userIsDebtor = string.Equals(debtorIban, account.Iban, StringComparison.OrdinalIgnoreCase);
                return (isIncoming: !userIsDebtor, determined: true);
            }

            return (isIncoming: transaction.CreditDebitIndicator != "DBIT", determined: true);
        }

        private static string CalculateTransactionId(int bankAccountId, Transaction transaction, decimal amount, DateTime bookingDate)
        {
            // entry_reference is only unique within a single account, so every
            // fingerprint is prefixed with the local BankAccount.Id to keep the
            // global TransactionId unique index collision-free across accounts.
            if (!string.IsNullOrEmpty(transaction.EntryReference))
            {
                return $"{bankAccountId}/{transaction.EntryReference}/{amount}/{transaction.TransactionAmount?.Currency}/{bookingDate:yyyy-MM-dd}";
            }

            if (!string.IsNullOrEmpty(transaction.ReferenceNumber))
            {
                return $"{bankAccountId}/{transaction.ReferenceNumber}/{amount}/{transaction.TransactionAmount?.Currency}/{bookingDate:yyyy-MM-dd}";
            }

            // Fuzzy fingerprint per EnableBanking docs when no stable reference exists
            return $"{bankAccountId}/{bookingDate:yyyy-MM-dd}/{amount}/{transaction.TransactionAmount?.Currency}/{transaction.CreditDebitIndicator}";
        }
    }
}
