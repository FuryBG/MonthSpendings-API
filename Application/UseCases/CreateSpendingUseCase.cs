using Application.Resources;
using Application.Contracts;
using Application.Dto.Budget;
using Application.Dto.Notification;
using Application.Enums;
using Application.Interfaces;
using Application.Mappers;
using Application.Services;
using Domain;
using Microsoft.Extensions.Logging;

namespace Application.UseCases
{
    public interface ICreateSpendingUseCase
    {
        Task<CaseResult<SpendingDto?>> InvokeAsync(SpendingDto spendingDto);
    }

    public class CreateSpendingUseCase : ICreateSpendingUseCase
    {
        private IUnitOfWork _UnitOfWork { get; set; }
        private IUserService _UserService { get; set; }
        private IPushNotificationService _PushNotificationService { get; set; }
        private readonly ILogger<CreateSpendingUseCase> _Logger;

        public CreateSpendingUseCase(IUnitOfWork unitOfWork, IUserService userService, IPushNotificationService pushNotificationService, ILogger<CreateSpendingUseCase> logger)
        {
            _UnitOfWork = unitOfWork;
            _UserService = userService;
            _PushNotificationService = pushNotificationService;
            _Logger = logger;
        }

        public async Task<CaseResult<SpendingDto?>> InvokeAsync(SpendingDto spendingDto)
        {
            var result = new CaseResult<SpendingDto?>();
            result.Successful = true;

            int userId = _UserService.GetUserId();
            BudgetCategory? budgetCategory = await _UnitOfWork.BudgetCategoryRepository.GetBudgetCategoryById(spendingDto.BudgetCategoryId, userId);

            if (budgetCategory == null)
            {
                _Logger.LogInformation("Category {CategoryId} not found for user {UserId} when creating spending", spendingDto.Id, userId);
                return CaseResult<SpendingDto?>.Error(Messages.CategoryNotFound);

            }

            decimal categoryBalance = budgetCategory.Spendings.Sum(s => s.Amount);
            decimal newBalance = categoryBalance + spendingDto.Amount;

            if (spendingDto.Amount < 0 && newBalance < 0)
            {
                _Logger.LogInformation("Insufficient balance: attempted {Attempted}, available {Available} in category {CategoryId}", spendingDto.Amount, categoryBalance, spendingDto.Id);
                return CaseResult<SpendingDto?>.Error(Messages.SpendingInsufficientBalance);
            }

            Spending newSpending = spendingDto.ToEntity();
            newSpending.CreatedByUserId = userId;
            Spending addedSpending = _UnitOfWork.CategorySpendingsRepository.AddSpending(newSpending);

            await _UnitOfWork.CommitAsync();

            List<AppUser> notificationReceivers = budgetCategory.Budget.Users.Where(u => u.Id != userId).ToList();
            AppUser currentUser = budgetCategory.Budget.Users.Where(u => u.Id == userId).First();
            result.Data = addedSpending.ToDto();
            result.Data.CreatedByEmail = currentUser.Email;
            result.Data.CreatedByName = $"{currentUser.FirstName} {currentUser.LastName}".Trim();
            await SendSpendingNotification(notificationReceivers, currentUser.Email, budgetCategory.Name, spendingDto.Amount, budgetCategory.Budget.Currency.Symbol);
            _Logger.LogInformation("Spending {SpendingId} created: {Amount} in category {CategoryId} by user {UserId}", result.Data!.Id, spendingDto.Amount, spendingDto.Id, userId);

            return result;
        }

        private async Task SendSpendingNotification(List<AppUser> receivers, string userName, string categoryName, decimal spentAmount, string currencySymbol)
        {
            bool added = spentAmount > 0;
            await _PushNotificationService.SendLocalized(
                receivers,
                added ? nameof(Messages.PushFundsAddedTitle) : nameof(Messages.PushFundsSpentTitle),
                added ? nameof(Messages.PushFundsAddedBody) : nameof(Messages.PushFundsSpentBody),
                new NotificationDto() { Type = NotificationTypeEnum.SpendingAdd },
                userName, Math.Abs(spentAmount), currencySymbol, categoryName);
        }
    }
}
