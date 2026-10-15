using Application.Resources;
using Application.Contracts;
using Application.Dto;
using Application.Interfaces;
using Application.Mappers;
using Application.Services;
using Application.Services.NotificationParsing;
using Domain;
using Domain.Bank;
using Microsoft.Extensions.Logging;

namespace Application.UseCases.NotificationTransactions
{
    public interface ICreateNotificationTransactionUseCase
    {
        Task<CaseResult<NotificationTransactionDto>> InvokeAsync(CreateNotificationTransactionDto dto, CancellationToken cancellationToken);
    }

    public class CreateNotificationTransactionUseCase : ICreateNotificationTransactionUseCase
    {
        // Wallet re-posts / updates the same notification; the phone may also retry from its outbox.
        private static readonly TimeSpan DuplicateWindow = TimeSpan.FromMinutes(2);

        private readonly IUnitOfWork _UnitOfWork;
        private readonly IUserService _UserService;
        private readonly IWalletNotificationParser _Parser;
        private readonly ILogger<CreateNotificationTransactionUseCase> _Logger;

        public CreateNotificationTransactionUseCase(IUnitOfWork unitOfWork, IUserService userService, IWalletNotificationParser parser, ILogger<CreateNotificationTransactionUseCase> logger)
        {
            _UnitOfWork = unitOfWork;
            _UserService = userService;
            _Parser = parser;
            _Logger = logger;
        }

        public async Task<CaseResult<NotificationTransactionDto>> InvokeAsync(CreateNotificationTransactionDto dto, CancellationToken cancellationToken)
        {
            int userId = _UserService.GetUserId();

            AppUser? user = await _UnitOfWork.UserRepository.GetUserById(userId);
            if (user == null)
            {
                _Logger.LogWarning("Notification transaction rejected: user {UserId} behind a valid token not found", userId);
                return CaseResult<NotificationTransactionDto>.Error(Messages.UserNotFound);
            }
            if (!user.SyncWalletTransactions)
            {
                _Logger.LogInformation("Notification transaction rejected: wallet sync disabled for user {UserId}", userId);
                return CaseResult<NotificationTransactionDto>.Error(Messages.WalletSyncDisabled, ErrorType.Forbidden);
            }

            DateTime now = DateTime.UtcNow;
            DateTime receivedAt = dto.PostedAt == default ? now : dto.PostedAt.ToUniversalTime();
            if (receivedAt > now)
                receivedAt = now;

            NotificationTransaction? duplicate = await _UnitOfWork.NotificationTransactionRepository
                .FindDuplicateAsync(userId, dto.Title, dto.Text, receivedAt, DuplicateWindow, cancellationToken);
            if (duplicate != null)
            {
                _Logger.LogInformation("Duplicate wallet notification for user {UserId} ignored, existing transaction {Id}", userId, duplicate.Id);
                return CaseResult<NotificationTransactionDto>.Success(duplicate.ToDto());
            }

            string? currencyHint = await _UnitOfWork.BudgetRepository.GetPrimaryCurrencyCode(userId);
            WalletNotificationParseResult parsed = _Parser.Parse(dto.Title, dto.Text, currencyHint);

            var transaction = new NotificationTransaction
            {
                UserId = userId,
                Amount = parsed.Amount,
                Currency = parsed.Currency,
                MerchantName = parsed.Merchant,
                CardLast4 = parsed.CardLast4,
                RawTitle = dto.Title,
                RawBody = dto.Text,
                PackageName = dto.PackageName,
                ParseFailed = !parsed.Success,
                ReceivedAt = receivedAt,
                Categorized = false,
            };

            await _UnitOfWork.NotificationTransactionRepository.AddAsync(transaction, cancellationToken);
            await _UnitOfWork.CommitAsync();

            if (!parsed.Success)
            {
                _Logger.LogInformation("Wallet notification {Id} for user {UserId} not parsed: {Reason}", transaction.Id, userId, parsed.FailureReason);
                return CaseResult<NotificationTransactionDto>.Success(transaction.ToDto());
            }

            // Auto-apply matching rule if one exists
            TransactionCategoryRule? rule = await _UnitOfWork.TransactionCategoryRuleRepository.FindMatchingRuleAsync(userId, transaction.MerchantName, cancellationToken);
            if (rule != null)
            {
                BudgetCategory? category = await _UnitOfWork.BudgetCategoryRepository.GetBudgetCategoryById(rule.CategoryId, userId);
                BudgetPeriod? budgetPeriod = category?.Budget.BudgetPeriods.FirstOrDefault();
                if (category != null && budgetPeriod != null)
                {
                    Spending spending = _UnitOfWork.CategorySpendingsRepository.AddSpending(new Spending
                    {
                        Amount = -transaction.Amount,
                        NotificationTransactionId = transaction.Id,
                        Date = transaction.ReceivedAt,
                        BudgetCategoryId = category.Id,
                        BudgetPeriodId = budgetPeriod.Id,
                        CreatedByUserId = userId,
                    });
                    await _UnitOfWork.CommitAsync();
                    await _UnitOfWork.NotificationTransactionRepository.CategorizeAsync(transaction.Id, spending.Id, cancellationToken);
                    transaction.Categorized = true;
                    _Logger.LogInformation("Auto-categorized notification transaction {Id} via rule for merchant {Merchant}", transaction.Id, transaction.MerchantName);
                }
            }

            _Logger.LogInformation("Created notification transaction {Id} for user {UserId}, merchant {Merchant}", transaction.Id, userId, transaction.MerchantName);
            return CaseResult<NotificationTransactionDto>.Success(transaction.ToDto());
        }
    }
}
