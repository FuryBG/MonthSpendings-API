using Application.Resources;
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

            int userId = _UserService.GetUserId();
            var budget = await _UnitOfWork.BudgetRepository.GetBudgetById(budgetId, userId);

            if (budget == null)
            {
                _Logger.LogInformation("Budget {BudgetId} not found for user {UserId} on kick member", budgetId, userId);
                return CaseResult<bool>.Error(Messages.BudgetNotFound);
            }

            if (budget.OwnerId != userId)
            {
                _Logger.LogWarning("User {UserId} tried to remove member {TargetUserId} from budget {BudgetId} without being the owner", userId, targetUserId, budgetId);
                return CaseResult<bool>.Error(Messages.BudgetNotOwner);
            }

            if (targetUserId == userId)
            {
                _Logger.LogInformation("Owner {UserId} tried to remove themselves from budget {BudgetId}", userId, budgetId);
                return CaseResult<bool>.Error(Messages.BudgetOwnerCannotBeRemoved);
            }

            var target = budget.Users.FirstOrDefault(u => u.Id == targetUserId);

            if (target == null)
            {
                _Logger.LogInformation("Member {TargetUserId} not found in budget {BudgetId} (kick requested by owner {UserId})", targetUserId, budgetId, userId);
                return CaseResult<bool>.Error(Messages.BudgetMemberNotFound);
            }

            budget.Users.Remove(target);
            await _UnitOfWork.CommitAsync();

            await _PushNotificationService.SendLocalized(
                [target],
                nameof(Messages.PushKickedTitle),
                nameof(Messages.PushKickedBody),
                new NotificationDto() { Type = NotificationTypeEnum.KickedFromBudget },
                budget.Name);

            result.Data = true;
            _Logger.LogInformation("User {TargetUserId} removed from budget {BudgetId} by owner {OwnerId}", targetUserId, budgetId, userId);

            return result;
        }
    }
}
