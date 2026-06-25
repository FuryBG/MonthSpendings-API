using Application.Contracts;
using Application.Dto;
using Application.Dto.Notification;
using Application.Enums;
using Application.Interfaces;
using Application.Mappers;
using Application.Options;
using Application.Services;
using Domain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.UseCases
{
    public interface IUpdateBudgetInviteResponseUseCase
    {
        Task<CaseResult<BudgetInviteDto?>> InvokeAsync(int budgetInviteId, bool accepted);
    }

    public class UpdateBudgetInviteResponseUseCase : IUpdateBudgetInviteResponseUseCase
    {
        private IUnitOfWork _UnitOfWork { get; set; }
        private IUserService _UserService { get; set; }
        private IPushNotificationService _PushNotificationService { get; set; }
        private readonly ILogger<UpdateBudgetInviteResponseUseCase> _Logger;
        private readonly PlanLimitsOptions _Limits;
        public UpdateBudgetInviteResponseUseCase(IUnitOfWork unitOfWork, IUserService userService, IPushNotificationService pushNotificationService, ILogger<UpdateBudgetInviteResponseUseCase> logger, IOptions<PlanLimitsOptions> planLimitsOptions)
        {
            _UnitOfWork = unitOfWork;
            _UserService = userService;
            _PushNotificationService = pushNotificationService;
            _Logger = logger;
            _Limits = planLimitsOptions.Value;
        }
        public async Task<CaseResult<BudgetInviteDto?>> InvokeAsync(int budgetInviteId, bool accepted)
        {
            var result = new CaseResult<BudgetInviteDto?>();
            result.Successful = true;

            try
            {
                int userId = _UserService.GetUserId();

                BudgetInvite? budgetInvite = await _UnitOfWork.BudgetInviteRepository.GetBudgetInviteById(budgetInviteId);

                if (budgetInvite == null)
                {
                    _Logger.LogWarning("Budget invite {InviteId} not found", budgetInviteId);
                    result.Successful = false;
                    result.ErrorMessage = "Can't find the Budget invite to.";
                    return result;
                }

                if (budgetInvite.ReceiverId != userId)
                {
                    _Logger.LogWarning("User {UserId} attempted to respond to invite {InviteId} but is not the receiver", userId, budgetInviteId);
                    result.Successful = false;
                    result.ErrorMessage = "Can't find the Budget invite to respond.";
                    return result;
                }

                Budget? budget = await _UnitOfWork.BudgetRepository.GetBudgetById(budgetInvite.BudgetId, budgetInvite.SenderId);

                if (budget == null)
                {
                    _Logger.LogWarning("Budget {BudgetId} not found when responding to invite {InviteId}", budgetInvite.BudgetId, budgetInviteId);
                    result.Successful = false;
                    result.ErrorMessage = "Can't find the Budget invite to respond.";
                    return result;
                }

                budgetInvite.Accepted = accepted;

                if (accepted)
                {
                    var receiverBudgets = await _UnitOfWork.BudgetRepository.GetUserBudgets(userId);

                    if (receiverBudgets.Count >= _Limits.FreeBudgetLimit && !budgetInvite.Receiver.IsPro)
                    {
                        result.Successful = false;
                        result.ErrorMessage = "To have more than one budget you must be a Pro.";
                        return result;
                    }

                    if (receiverBudgets.Count >= _Limits.ProBudgetLimit && budgetInvite.Receiver.IsPro)
                    {
                        result.Successful = false;
                        result.ErrorMessage = $"Pro accounts are limited to {_Limits.ProBudgetLimit} budgets.";
                        return result;
                    }

                    if (budget.Users.Count > _Limits.FreeParticipantJoinThreshold && !budgetInvite.Receiver.IsPro)
                    {
                        result.Successful = false;
                        result.ErrorMessage = "You must be a Pro to join a budget with more than two participants.";
                        return result;
                    }

                    if (budget.Users.Count >= _Limits.ProParticipantLimit)
                    {
                        result.Successful = false;
                        result.ErrorMessage = $"This budget has reached the {_Limits.ProParticipantLimit}-participant limit.";
                        return result;
                    }

                    budget.Users.Add(budgetInvite.Receiver);
                }

                BudgetInvite createdInvite = _UnitOfWork.BudgetInviteRepository.UpdateInvite(budgetInvite);

                await _UnitOfWork.CommitAsync();
                await SendBudgetInviteNotification(budgetInvite.Sender.NotificationToken, budgetInvite.Receiver.Email, budgetInvite.Accepted.Value);

                result.Data = createdInvite.ToDto();
                _Logger.LogInformation("Budget invite {InviteId} responded by user {UserId}", budgetInviteId, userId);
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "Error updating budget invite {InviteId} response", budgetInviteId);
                result.Successful = false;
                result.ErrorMessage = "Something got wrong during respond to Invite. Please try again later.";
            }

            return result;
        }

        private async Task SendBudgetInviteNotification(string receiverNotificationToken, string receiverEmail, bool accepted)
        {
            string notificationMessage = accepted
                ? $"The invite for a budget you sent is accepted by {receiverEmail}!"
                : $"The invite for a budget you sent is declined by {receiverEmail}!";
            await _PushNotificationService.SendNotification([receiverNotificationToken], "Budget Invite Status", notificationMessage, new NotificationDto() { Type = NotificationTypeEnum.InviteResponse });
        }
    }
}
