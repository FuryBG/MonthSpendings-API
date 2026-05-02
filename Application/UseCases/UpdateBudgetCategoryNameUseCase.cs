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
    public interface IUpdateBudgetCategoryNameUseCase
    {
        Task<CaseResult<BudgetCategoryDto?>> InvokeAsync(int budgetCategoryId, string newName);
    }

    public class UpdateBudgetCategoryNameUseCase : IUpdateBudgetCategoryNameUseCase
    {
        private IUnitOfWork _UnitOfWork { get; set; }
        private IUserService _UserService { get; set; }
        private IPushNotificationService _PushNotificationService { get; set; }
        private readonly ILogger<UpdateBudgetCategoryNameUseCase> _Logger;
        public UpdateBudgetCategoryNameUseCase(IUnitOfWork unitOfWork, IUserService userService, IPushNotificationService pushNotificationService, ILogger<UpdateBudgetCategoryNameUseCase> logger)
        {
            _UnitOfWork = unitOfWork;
            _UserService = userService;
            _PushNotificationService = pushNotificationService;
            _Logger = logger;
        }
        public async Task<CaseResult<BudgetCategoryDto?>> InvokeAsync(int budgetCategoryId, string newName)
        {
            var result = new CaseResult<BudgetCategoryDto?>();
            result.Successful = true;

            try
            {
                int userId = _UserService.GetUserId();
                AppUser? user = await _UnitOfWork.UserRepository.GetUserById(userId);

                if (user == null)
                {
                    _Logger.LogWarning("User {UserId} not found when updating category name", userId);
                    result.Successful = false;
                    result.ErrorMessage = "Can't update the Budget Category.";
                    return result;
                }

                BudgetCategory? budgetCategory = await _UnitOfWork.BudgetCategoryRepository.GetBudgetCategoryById(budgetCategoryId, userId);

                if (budgetCategory == null)
                {
                    _Logger.LogWarning("Budget category {CategoryId} not found for name update", budgetCategoryId);
                    result.Successful = false;
                    result.ErrorMessage = "Can't find the Budget Category.";
                    return result;
                }

                string oldName = budgetCategory.Name;
                budgetCategory.Name = newName;
                _UnitOfWork.BudgetCategoryRepository.UpdateCategory(budgetCategory);

                await _UnitOfWork.CommitAsync();
                result.Data = budgetCategory.ToDto();
                _Logger.LogInformation("Category {CategoryId} renamed to {NewName} by user {UserId}", budgetCategoryId, newName, userId);

                List<string> notificationReceiversTokens = budgetCategory.Budget.Users.Select(u => u.NotificationToken).ToList();
                await SendBudgetUpdateNotification(notificationReceiversTokens, user.Email, oldName, newName);
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "Error updating category {CategoryId} name", budgetCategoryId);
                result.Successful = false;
                result.ErrorMessage = "Something got wrong during respond to Invite. Please try again later.";
            }
            return result;
        }

        private async Task SendBudgetUpdateNotification(List<string> receiverNotificationTokens, string initiatorEmail, string oldCategoryName, string newCategoryName)
        {
            string notificationMessage = $"{initiatorEmail} updated category name from {oldCategoryName} to {newCategoryName}.";
            await _PushNotificationService.SendNotification(receiverNotificationTokens, "Budget Category Update", notificationMessage, new NotificationDto() { Type = NotificationTypeEnum.BudgetCategoryUpdate });
        }
    }
}
