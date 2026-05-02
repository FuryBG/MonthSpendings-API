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

            int userId = 0;
            try
            {
                userId = _UserService.GetUserId();
                BudgetCategory? budgetCategory = await _UnitOfWork.BudgetCategoryRepository.GetBudgetCategoryById(spendingDto.BudgetCategoryId, userId);

                if (budgetCategory == null)
                {
                    _Logger.LogWarning("Category {CategoryId} not found for user {UserId} when creating spending", spendingDto.Id, userId);
                    result.Successful = false;
                    result.ErrorMessage = "Can't find the category to add spending.";
                    return result;

                }

                decimal categoryBalance = budgetCategory.Spendings.Sum(s => s.Amount);
                decimal newBalance = categoryBalance + spendingDto.Amount;

                if (spendingDto.Amount < 0 && newBalance < 0)
                {
                    _Logger.LogWarning("Insufficient balance: attempted {Attempted}, available {Available} in category {CategoryId}", spendingDto.Amount, categoryBalance, spendingDto.Id);
                    result.Successful = false;
                    result.ErrorMessage = "Trying to spend more than the category balance.";
                    return result;
                }

                await _UnitOfWork.BeginTransactionAsync();

                Spending newSpending = spendingDto.ToEntity();
                newSpending.CreatedByUserId = userId;
                Spending addedSpending = _UnitOfWork.CategorySpendingsRepository.AddSpending(newSpending);

                await _UnitOfWork.CommitAsync();

                if (addedSpending.BankTransactionId != null)
                {
                    await _UnitOfWork.BankTransactionRepository.CategorizeAsync([addedSpending.BankTransactionId.Value], addedSpending.Id, new CancellationToken());
                }

                await _UnitOfWork.CommitTransactionAsync();

                List<string> budgetUsersNotificationTokens = budgetCategory.Budget.Users.Where(u => u.Id != userId).Select(u => u.NotificationToken).ToList();
                AppUser currentUser = budgetCategory.Budget.Users.Where(u => u.Id == userId).First();
                result.Data = addedSpending.ToDto();
                result.Data.CreatedByEmail = currentUser.Email;
                result.Data.CreatedByName = $"{currentUser.FirstName} {currentUser.LastName}".Trim();
                await SendSpendingNotification(budgetUsersNotificationTokens, currentUser.Email, budgetCategory.Budget.Name, budgetCategory.Name, spendingDto.Amount);
                _Logger.LogInformation("Spending {SpendingId} created: {Amount} in category {CategoryId} by user {UserId}", result.Data!.Id, spendingDto.Amount, spendingDto.Id, userId);
            }
            catch (Exception ex)
            {
                await _UnitOfWork.RollbackTransactionAsync();
                _Logger.LogError(ex, "Error creating spending for user {UserId}", userId);
                result.Successful = false;
                result.ErrorMessage = "Something got wrong during getting budgets. Please try again later.";
            }

            return result;
        }

        private async Task SendSpendingNotification(List<string> receiversNotificationToken, string userName, string budgetName, string categoryName, decimal spentAmound)
        {
            string notificationMessage = spentAmound > 0 ?
                $"{userName} Added {spentAmound} to {categoryName}." :
                $"{userName} Spent {spentAmound} from {categoryName}.";

            string notificationTitle = spentAmound > 0 ?
                $"Funds added." :
                $"Funds spent.";

            await _PushNotificationService.SendNotification(receiversNotificationToken, notificationTitle, notificationMessage, new NotificationDto() { Type = NotificationTypeEnum.SpendingAdd });
        }
    }
}
