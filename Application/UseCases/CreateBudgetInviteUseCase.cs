using Application.Resources;
using Application.Contracts;
using Application.Dto;
using Application.Dto.Notification;
using Application.Enums;
using Application.Interfaces;
using Application.Mappers;
using Application.Services;
using Domain;
using Microsoft.Extensions.Logging;

namespace Application.UseCases
{
    public interface ICreateBudgetInviteUseCase
    {
        Task<CaseResult<BudgetInviteDto?>> InvokeAsync(BudgetInviteDto budgetInviteDto);
    }

    public class CreateBudgetInviteUseCase : ICreateBudgetInviteUseCase
    {
        private IUnitOfWork _UnitOfWork { get; set; }
        private IUserService _UserService { get; set; }
        private IPushNotificationService _PushNotificationService { get; set; }
        private readonly ILogger<CreateBudgetInviteUseCase> _Logger;
        public CreateBudgetInviteUseCase(IUnitOfWork unitOfWork, IUserService userService, IPushNotificationService pushNotificationService, ILogger<CreateBudgetInviteUseCase> logger)
        {
            _UnitOfWork = unitOfWork;
            _UserService = userService;
            _PushNotificationService = pushNotificationService;
            _Logger = logger;
        }
        public async Task<CaseResult<BudgetInviteDto?>> InvokeAsync(BudgetInviteDto budgetInviteDto)
        {
            var result = new CaseResult<BudgetInviteDto?>();
            result.Successful = true;

            int userId = _UserService.GetUserId();
            Budget? budget = await _UnitOfWork.BudgetRepository.GetBudgetById(budgetInviteDto.BudgetId, userId);

            if (budget == null)
            {
                _Logger.LogInformation("Budget {BudgetId} not found for user {UserId} when creating invite", budgetInviteDto.BudgetId, userId);
                return CaseResult<BudgetInviteDto?>.Error(Messages.BudgetNotFound);
            }

            AppUser? sender = await _UnitOfWork.UserRepository.GetUserById(userId);

            if (sender == null)
            {
                _Logger.LogWarning("Sender {SenderId} not found when creating budget invite", userId);
                return CaseResult<BudgetInviteDto?>.Error(Messages.UserInvalid);
            }

            AppUser? receiver = await _UnitOfWork.UserRepository.GetUserByEmail(budgetInviteDto.ReceiverEmail);

            if (receiver == null)
            {
                _Logger.LogInformation("Invite to budget {BudgetId} by user {UserId} rejected: receiver email is not registered", budgetInviteDto.BudgetId, userId);
                return CaseResult<BudgetInviteDto?>.Error(Messages.InviteUserNotFound);
            }

            BudgetInvite budgetInvite = budgetInviteDto.ToEntity();
            budgetInvite.ReceiverId = receiver.Id;
            budgetInvite.SenderId = sender.Id;

            BudgetInvite createdInvite = _UnitOfWork.BudgetInviteRepository.CreateInvite(budgetInvite);

            await _UnitOfWork.CommitAsync();

            await SendBudgetInviteNotification(receiver);

            result.Data = createdInvite.ToDto();
            _Logger.LogInformation("Budget invite {InviteId} sent from {SenderId} to {ReceiverId} for budget {BudgetId}", result.Data!.Id, userId, budgetInviteDto.ReceiverEmail, budgetInviteDto.BudgetId);

            return result;
        }

        private async Task SendBudgetInviteNotification(AppUser receiver)
        {
            await _PushNotificationService.SendLocalized([receiver], nameof(Messages.PushInviteReceivedTitle), nameof(Messages.PushInviteReceivedBody), new NotificationDto() { Type = NotificationTypeEnum.ReceivedInvite });
        }
    }
}
