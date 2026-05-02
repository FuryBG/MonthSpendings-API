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

            try
            {
                int userId = _UserService.GetUserId();
                Budget? budget = await _UnitOfWork.BudgetRepository.GetBudgetById(budgetInviteDto.BudgetId, userId);

                if (budget == null)
                {
                    _Logger.LogWarning("Budget {BudgetId} not found when creating invite", budgetInviteDto.BudgetId);
                    result.Successful = false;
                    result.ErrorMessage = "Can't find the Budget to send Invite.";
                    return result;
                }

                AppUser? sender = await _UnitOfWork.UserRepository.GetUserById(userId);

                if (sender == null)
                {
                    _Logger.LogWarning("Sender {SenderId} not found when creating budget invite", userId);
                    result.Successful = false;
                    result.ErrorMessage = "Corrupted user, please log in again, and send invite again.";
                    return result;
                }

                AppUser? receiver = await _UnitOfWork.UserRepository.GetUserByEmail(budgetInviteDto.ReceiverEmail);

                if (receiver == null || receiver.NotificationToken == null || receiver.NotificationToken == string.Empty)
                {
                    _Logger.LogWarning("Receiver {ReceiverId} not found when creating budget invite", budgetInviteDto.ReceiverEmail);
                    result.Successful = false;
                    result.ErrorMessage = "User with this email doesn't exist.";
                    return result;
                }

                BudgetInvite budgetInvite = budgetInviteDto.ToEntity();
                budgetInvite.ReceiverId = receiver.Id;
                budgetInvite.SenderId = sender.Id;

                BudgetInvite createdInvite = _UnitOfWork.BudgetInviteRepository.CreateInvite(budgetInvite);

                await _UnitOfWork.CommitAsync();
                await SendBudgetInviteNotification(receiver.NotificationToken);

                result.Data = createdInvite.ToDto();
                _Logger.LogInformation("Budget invite {InviteId} sent from {SenderId} to {ReceiverId} for budget {BudgetId}", result.Data!.Id, userId, budgetInviteDto.ReceiverEmail, budgetInviteDto.BudgetId);
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "Error creating budget invite for budget {BudgetId}", budgetInviteDto.BudgetId);
                result.Successful = false;
                result.ErrorMessage = "Something got wrong during creating Invite. Please try again later.";
            }

            return result;
        }

        private async Task SendBudgetInviteNotification(string receiverNotificationToken)
        {
            await _PushNotificationService.SendNotification([receiverNotificationToken], "Budget Invite", "You have been invited for a Budget. Click to see the invite.", new NotificationDto() { Type = NotificationTypeEnum.ReceivedInvite });
        }
    }
}
