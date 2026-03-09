using Application.Interfaces;
using Domain.Bank;
using Domain.Bank.Enums;
using EnableBanking.Interfaces;
using EnableBanking.Models;
using EnableBanking.Models.Accounts;
using EnableBanking.Models.Payments;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Application.BackgroundWorkers
{
    public interface IBankSyncWorker
    {
        Task SyncBankAccountsAsync(CancellationToken ct);
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
            bool intervalSet = int.TryParse(_Configuration.GetSection("EnableBanking:TransactionSyncInterval").Value, out int updateInterval);

            if (!intervalSet)
            {
                throw new ApplicationException("EnableBanking:TransactionSyncInterval is not set.");
            }

            var threshold = DateTime.UtcNow.AddMinutes(-updateInterval);
            List<BankConsent> bankConsents = await _UnitOfWork.BankConsentRepository.GetConsentsForSync(threshold, cancellationToken);

            try
            {
                foreach (BankConsent bankConsent in bankConsents)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        break;
                    }

                    foreach (BankAccount account in bankConsent.Accounts)
                    {
                        GetTransactionsRequest request = new GetTransactionsRequest() { AccountId = account.AccountUuid, TransactionStatus = TransactionStatus.BOOK.ToString() };
                        ApiResponse<GetTransactionsResponse> response = await _AccountService.GetTransactionsAsync(request, cancellationToken);

                        if (response.StatusCode != HttpStatusCode.OK || response.Error != null ||
                            response.Data == null || response.Data.Transactions == null)
                        {
                            _Logger.LogCritical($"Cannot get transactions for account with id: {account.Id}. Error message: {response.Error?.Message}");
                            continue;
                        }

                        await _UnitOfWork.BeginTransactionAsync();

                        foreach (Transaction transaction in response.Data.Transactions)
                        {
                            bool isAmountParsed = decimal.TryParse(transaction.TransactionAmount?.Amount, out decimal amount);
                            bool bookingDateParsed = DateTime.TryParse(transaction.BookingDate, out DateTime bookingDate);
                            bool statusParsed = Enum.TryParse(transaction.Status, ignoreCase: true, out TransactionStatus transactionStatus);

                            if (!isAmountParsed || !bookingDateParsed || !statusParsed)
                            {
                                _Logger.LogWarning($"Cannot parse transaction details amount: {transaction.TransactionAmount?.Amount}, bookingDate: {transaction.BookingDate}, status: {transaction.Status}");
                                continue;
                            }

                            string transactionId = CalculateTransactionId(transaction, amount, bookingDate);

                            BankTransaction bankTransaction = new BankTransaction()
                            {
                                Currency = account.Currency,
                                Amount = amount,
                                MerchantCode = transaction.MerchantCategoryCode,
                                BookingDate = DateTime.SpecifyKind(bookingDate, DateTimeKind.Utc),
                                TransactionId = transactionId,
                                Status = transactionStatus,
                                BankAccountId = account.Id,
                            };

                            await _UnitOfWork.BankTransactionRepository.AddTransaction(bankTransaction, cancellationToken);
                        }
                        await _UnitOfWork.CommitTransactionAsync();
                    }
                    await _UnitOfWork.BankConsentRepository.MarkConsentsAsSyncedAsync([bankConsent.Id], DateTime.UtcNow, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, ex.Message);
                await _UnitOfWork.RollbackTransactionAsync();
            }
        }

        private string CalculateTransactionId(Transaction transaction, decimal amount, DateTime bookingDate)
        {
            string transactionId = "";
            if (!string.IsNullOrEmpty(transaction.EntryReference))
            {
                transactionId += $"{transaction.EntryReference}/";
            }
            else if (!string.IsNullOrEmpty(transaction.ReferenceNumber))
            {
                transactionId = $"{transaction.ReferenceNumber}/";
            }
            else if (!string.IsNullOrEmpty(transaction.TransactionId))
            {
                transactionId = $"{transaction.TransactionId}/";
            }

            transactionId += $"{amount}/";
            transactionId += $"{transaction.TransactionAmount?.Currency}";
            transactionId += $"{bookingDate.ToString("yyyy/MM/dd/hh/mm/ss")}";

            return transactionId;
        }
    }
}