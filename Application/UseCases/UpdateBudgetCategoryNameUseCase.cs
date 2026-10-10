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

            int userId = _UserService.GetUserId();
            AppUser? user = await _UnitOfWork.UserRepository.GetUserById(userId);

            if (user == null)
            {
                _Logger.LogWarning("User {UserId} not found when updating category name", userId);
                return CaseResult<BudgetCategoryDto?>.Error(Messages.UserInvalid);
            }

            BudgetCategory? budgetCategory = await _UnitOfWork.BudgetCategoryRepository.GetBudgetCategoryById(budgetCategoryId, userId);

            if (budgetCategory == null)
            {
                _Logger.LogInformation("Budget category {CategoryId} not found for user {UserId} on rename", budgetCategoryId, userId);
                return CaseResult<BudgetCategoryDto?>.Error(Messages.CategoryNotFound);
            }

            string oldName = budgetCategory.Name;
            budgetCategory.Name = newName;
            _UnitOfWork.BudgetCategoryRepository.UpdateCategory(budgetCategory);

            await _UnitOfWork.CommitAsync();
            result.Data = budgetCategory.ToDto();
            _Logger.LogInformation("Category {CategoryId} renamed to {NewName} by user {UserId}", budgetCategoryId, newName, userId);

            await SendBudgetUpdateNotification(budgetCategory.Budget.Users, user.Email, oldName, newName);
            return result;
        }

        private async Task SendBudgetUpdateNotification(List<AppUser> receivers, string initiatorEmail, string oldCategoryName, string newCategoryName)
        {
            await _PushNotificationService.SendLocalized(
                receivers,
                nameof(Messages.PushCategoryRenamedTitle),
                nameof(Messages.PushCategoryRenamedBody),
                new NotificationDto() { Type = NotificationTypeEnum.BudgetCategoryUpdate },
                initiatorEmail, oldCategoryName, newCategoryName);
        }
    }
}
