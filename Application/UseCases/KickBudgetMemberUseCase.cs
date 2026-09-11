using Application.Contracts;
using Application.Dto.Notification;
using Application.Enums;
using Application.Interfaces;
using Application.Services;
using Microsoft.Extensions.Logging;

namespace Application.UseCases
{
    public interface IKickBudgetMemberUseCase
    {
        Task<CaseResult<bool>> InvokeAsync(int budgetId, int targetUserId);
    }

    public class KickBudgetMemberUseCase : IKickBudgetMemberUseCase
    {
        private IUnitOfWork _UnitOfWork { get; set; }
        private IUserService _UserService { get; set; }
        private IPushNotificationService _PushNotificationService { get; set; }
        private readonly ILogger<KickBudgetMemberUseCase> _Logger;

        public KickBudgetMemberUseCase(IUnitOfWork unitOfWork, IUserService userService, IPushNotificationService pushNotificationService, ILogger<KickBudgetMemberUseCase> logger)
        {
            _UnitOfWork = unitOfWork;
            _UserService = userService;
            _PushNotificationService = pushNotificationService;
            _Logger = logger;
        }

        public async Task<CaseResult<bool>> InvokeAsync(int budgetId, int targetUserId)
        {
            var result = new CaseResult<bool>();
            result.Successful = true;

            try
            {
                int userId = _UserService.GetUserId();
                var budget = await _UnitOfWork.BudgetRepository.GetBudgetById(budgetId, userId);

                if (budget == null)
                {
                    result.Successful = false;
                    result.ErrorMessage = "Budget not found.";
                    return result;
                }

                if (budget.OwnerId != userId)
                {
                    result.Successful = false;
                    result.ErrorMessage = "Only the budget owner can remove members.";
                    return result;
                }

                if (targetUserId == userId)
                {
                    result.Successful = false;
                    result.ErrorMessage = "The owner cannot be removed from the budget.";
                    return result;
                }

                var target = budget.Users.FirstOrDefault(u => u.Id == targetUserId);

                if (target == null)
                {
                    result.Successful = false;
                    result.ErrorMessage = "User is not a member of this budget.";
                    return result;
                }

                budget.Users.Remove(target);
                await _UnitOfWork.CommitAsync();

                if (!string.IsNullOrEmpty(target.NotificationToken))
                {
                    await _PushNotificationService.SendNotification(
                        [target.NotificationToken],
                        "Removed from budget",
                        $"You have been removed from \"{budget.Name}\".",
                        new NotificationDto() { Type = NotificationTypeEnum.KickedFromBudget }
                    );
                }

                result.Data = true;
                _Logger.LogInformation("User {TargetUserId} removed from budget {BudgetId} by owner {OwnerId}", targetUserId, budgetId, userId);
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "Error removing user {TargetUserId} from budget {BudgetId}", targetUserId, budgetId);
                result.Successful = false;
                result.ErrorMessage = "Something went wrong. Please try again later.";
            }

            return result;
        }
    }
}
