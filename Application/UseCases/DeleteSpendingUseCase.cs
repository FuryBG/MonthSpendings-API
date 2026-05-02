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

            try
            {
                int userId = _UserService.GetUserId();
                Spending? spending = await _UnitOfWork.CategorySpendingsRepository.GetSpending(spendingId, userId);

                if (spending == null)
                {
                    result.Successful = false;
                    result.ErrorMessage = $"Can't find spending with id {spendingId} to delete. Please try again later.";
                    return result;
                }

                _UnitOfWork.CategorySpendingsRepository.DeleteSpending(spending);
                await _UnitOfWork.CommitAsync();
                result.Data = spendingId;

                List<string> budgetUsersNotificationTokens = spending.BudgetCategory.Budget.Users.Where(u => u.Id != userId).Select(u => u.NotificationToken).ToList();
                AppUser currentUser = spending.BudgetCategory.Budget.Users.Where(u => u.Id == userId).First();
                await SendDeleteSpendingNotification(budgetUsersNotificationTokens, currentUser.Email, spending.BudgetCategory.Budget.Name, spending.BudgetCategory.Name, spending.Amount);
                _Logger.LogInformation("Spending {SpendingId} deleted by user {UserId}", spendingId, userId);
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "Error deleting spending {SpendingId}", spendingId);
                result.Successful = false;
                result.ErrorMessage = $"Something got wrong during deleting spending with id {spendingId}. Please try again later.";
            }

            return result;
        }

        private async Task SendDeleteSpendingNotification(List<string> receiversNotificationToken, string userName, string budgetName, string categoryName, decimal spentAmound)
        {
            string notificationMessage = $"{userName} spending with amount: {spentAmound} from {categoryName}.";
            await _PushNotificationService.SendNotification(receiversNotificationToken, "Spending deleted", notificationMessage, new NotificationDto() { Type = NotificationTypeEnum.SpendingDelete });
        }
    }
}
