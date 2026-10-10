using Application.Resources;
using Application.Contracts;
using Application.Dto.Notification;
using Application.Enums;
using Application.Interfaces;
using Application.Services;
using Domain;
using Microsoft.Extensions.Logging;

namespace Application.UseCases
{
    public interface IDeleteSpendingUseCase
    {
        Task<CaseResult<int?>> InvokeAsync(int spendingId);
    }

    public class DeleteSpendingUseCase : IDeleteSpendingUseCase
    {
        private IUnitOfWork _UnitOfWork { get; set; }
        private IUserService _UserService { get; set; }
        private IPushNotificationService _PushNotificationService { get; set; }
        private readonly ILogger<DeleteSpendingUseCase> _Logger;

        public DeleteSpendingUseCase(IUnitOfWork unitOfWork, IUserService userService, IPushNotificationService pushNotificationService, ILogger<DeleteSpendingUseCase> logger)
        {
            _UnitOfWork = unitOfWork;
            _UserService = userService;
            _PushNotificationService = pushNotificationService;
            _Logger = logger;
        }

        public async Task<CaseResult<int?>> InvokeAsync(int spendingId)
        {
            var result = new CaseResult<int?>();
            result.Successful = true;

            int userId = _UserService.GetUserId();
            Spending? spending = await _UnitOfWork.CategorySpendingsRepository.GetSpending(spendingId, userId);

            if (spending == null)
            {
                _Logger.LogInformation("Spending {SpendingId} not found for user {UserId} on delete", spendingId, userId);
                return CaseResult<int?>.Error(Messages.SpendingNotFound);
            }

            _UnitOfWork.CategorySpendingsRepository.DeleteSpending(spending);
            await _UnitOfWork.CommitAsync();
            result.Data = spendingId;

            List<AppUser> notificationReceivers = spending.BudgetCategory.Budget.Users.Where(u => u.Id != userId).ToList();
            AppUser currentUser = spending.BudgetCategory.Budget.Users.Where(u => u.Id == userId).First();
            await SendDeleteSpendingNotification(notificationReceivers, currentUser.Email, spending.BudgetCategory.Name, spending.Amount, spending.BudgetCategory.Budget.Currency.Symbol);
            _Logger.LogInformation("Spending {SpendingId} deleted by user {UserId}", spendingId, userId);

            return result;
        }

        private async Task SendDeleteSpendingNotification(List<AppUser> receivers, string userName, string categoryName, decimal spentAmount, string currencySymbol)
        {
            await _PushNotificationService.SendLocalized(
                receivers,
                nameof(Messages.PushSpendingDeletedTitle),
                nameof(Messages.PushSpendingDeletedBody),
                new NotificationDto() { Type = NotificationTypeEnum.SpendingDelete },
                userName, Math.Abs(spentAmount), currencySymbol, categoryName);
        }
    }
}
