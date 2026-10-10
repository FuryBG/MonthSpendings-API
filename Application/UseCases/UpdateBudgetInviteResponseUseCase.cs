using Application.Resources;
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

            int userId = _UserService.GetUserId();

            BudgetInvite? budgetInvite = await _UnitOfWork.BudgetInviteRepository.GetBudgetInviteById(budgetInviteId);

            if (budgetInvite == null)
            {
                _Logger.LogInformation("Budget invite {InviteId} not found (user {UserId})", budgetInviteId, userId);
                return CaseResult<BudgetInviteDto?>.Error(Messages.InviteNotFound);
            }

            if (budgetInvite.ReceiverId != userId)
            {
                _Logger.LogWarning("User {UserId} attempted to respond to invite {InviteId} but is not the receiver", userId, budgetInviteId);
                return CaseResult<BudgetInviteDto?>.Error(Messages.InviteNotFound);
            }

            Budget? budget = await _UnitOfWork.BudgetRepository.GetBudgetById(budgetInvite.BudgetId, budgetInvite.SenderId);

            if (budget == null)
            {
                _Logger.LogWarning("Budget {BudgetId} not found when responding to invite {InviteId}", budgetInvite.BudgetId, budgetInviteId);
                return CaseResult<BudgetInviteDto?>.Error(Messages.InviteNotFound);
            }

            budgetInvite.Accepted = accepted;

            if (accepted)
            {
                var receiverBudgets = await _UnitOfWork.BudgetRepository.GetUserBudgets(userId);

                if (receiverBudgets.Count >= _Limits.FreeBudgetLimit && !budgetInvite.Receiver.IsPro)
                {
                    _Logger.LogInformation("Non-pro user {UserId} needs Pro to accept invite {InviteId}: already has {BudgetCount} budgets", userId, budgetInviteId, receiverBudgets.Count);
                    return CaseResult<BudgetInviteDto?>.Error(Messages.BudgetProRequiredForMultiple, ErrorType.ProRequired);
                }

                if (receiverBudgets.Count >= _Limits.ProBudgetLimit && budgetInvite.Receiver.IsPro)
                {
                    _Logger.LogInformation("Pro user {UserId} cannot accept invite {InviteId}: reached the {Limit}-budget limit", userId, budgetInviteId, _Limits.ProBudgetLimit);
                    return CaseResult<BudgetInviteDto?>.Error(string.Format(Messages.BudgetProLimitReached, _Limits.ProBudgetLimit));
                }

                if (budget.Users.Count > _Limits.FreeParticipantJoinThreshold && !budgetInvite.Receiver.IsPro)
                {
                    _Logger.LogInformation("Non-pro user {UserId} needs Pro to accept invite {InviteId}: budget {BudgetId} has {MemberCount} members", userId, budgetInviteId, budget.Id, budget.Users.Count);
                    return CaseResult<BudgetInviteDto?>.Error(Messages.InviteProRequiredForLargeBudget, ErrorType.ProRequired);
                }

                if (budget.Users.Count >= _Limits.ProParticipantLimit)
                {
                    _Logger.LogInformation("User {UserId} cannot accept invite {InviteId}: budget {BudgetId} reached the {Limit}-member limit", userId, budgetInviteId, budget.Id, _Limits.ProParticipantLimit);
                    return CaseResult<BudgetInviteDto?>.Error(string.Format(Messages.InviteParticipantLimitReached, _Limits.ProParticipantLimit));
                }

                budget.Users.Add(budgetInvite.Receiver);
            }

            BudgetInvite createdInvite = _UnitOfWork.BudgetInviteRepository.UpdateInvite(budgetInvite);

            await _UnitOfWork.CommitAsync();
            await SendBudgetInviteNotification(budgetInvite.Sender, budgetInvite.Receiver.Email, budgetInvite.Accepted.Value);

            result.Data = createdInvite.ToDto();
            _Logger.LogInformation("Budget invite {InviteId} responded by user {UserId}", budgetInviteId, userId);

            return result;
        }

        private async Task SendBudgetInviteNotification(AppUser sender, string receiverEmail, bool accepted)
        {
            string bodyKey = accepted ? nameof(Messages.PushInviteAcceptedBody) : nameof(Messages.PushInviteDeclinedBody);
            await _PushNotificationService.SendLocalized([sender], nameof(Messages.PushInviteResponseTitle), bodyKey, new NotificationDto() { Type = NotificationTypeEnum.InviteResponse }, receiverEmail);
        }
    }
}
