using Application.Contracts;
using Application.Dto.Budget;
using Application.Dto.Notification;
using Application.Enums;
using Application.Interfaces;
using Application.Mappers;
using Application.Services;
using Domain;

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
        public CreateSpendingUseCase(IUnitOfWork unitOfWork, IUserService userService, IPushNotificationService pushNotificationService)
        {
            _UnitOfWork = unitOfWork;
            _UserService = userService;
            _PushNotificationService = pushNotificationService;
        }

        public async Task<CaseResult<SpendingDto?>> InvokeAsync(SpendingDto spendingDto)
        {
            var result = new CaseResult<SpendingDto?>();
            result.Successful = true;

            try
            {
                int userId = _UserService.GetUserId();
                BudgetCategory? budgetCategory = await _UnitOfWork.BudgetCategoryRepository.GetBudgetCategoryById(spendingDto.BudgetCategoryId, userId);

                if (budgetCategory == null)
                {
                    Console.WriteLine($"Can't find category with id {spendingDto.Id} to add spending.");
                    result.Successful = false;
                    result.ErrorMessage = "Can't find the category to add spending.";
                    return result;

                }

                decimal categoryBalance = budgetCategory.Spendings.Sum(s => s.Amount);
                decimal newBalance = categoryBalance + spendingDto.Amount;

                if (spendingDto.Amount < 0 && newBalance < 0)
                {
                    Console.WriteLine($"Trying to spend: {spendingDto.Amount} but the balance is: {categoryBalance}");
                    result.Successful = false;
                    result.ErrorMessage = "Trying to spend more than the category balance.";
                    return result;
                }

                Spending addedSpending = _UnitOfWork.CategorySpendingsRepository.AddSpending(spendingDto.ToEntity());
                await _UnitOfWork.CommitAsync();
                result.Data = addedSpending.ToDto();

                List<string> budgetUsersNotificationTokens = budgetCategory.Budget.Users.Where(u => u.Id != userId).Select(u => u.NotificationToken).ToList();
                AppUser currentUser = budgetCategory.Budget.Users.Where(u => u.Id == userId).First();
                await SendSpendingNotification(budgetUsersNotificationTokens, currentUser.Email, budgetCategory.Budget.Name, budgetCategory.Name, spendingDto.Amount);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
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
