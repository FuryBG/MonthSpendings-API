using Application.Contracts;
using Application.Dto;
using Application.Dto.Notification;
using Application.Enums;
using Application.Interfaces;
using Application.Mappers;
using Application.Services;
using Domain;

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
        private IPushNotificationsService _PushNotificationsService { get; set; }
        public UpdateBudgetInviteResponseUseCase(IUnitOfWork unitOfWork, IUserService userService, IPushNotificationsService pushNotificationService)
        {
            _UnitOfWork = unitOfWork;
            _UserService = userService;
            _PushNotificationsService = pushNotificationService;
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
                    Console.WriteLine($"Can't find budget invite with id {budgetInviteId} to respond for invite.");
                    result.Successful = false;
                    result.ErrorMessage = "Can't find the Budget invite to.";
                    return result;
                }

                if (budgetInvite.ReceiverId != userId)
                {
                    Console.WriteLine($"Logged user id is different than the budget invite receiver id. Logged user id: {userId} -- Invite receiver id: {budgetInvite.ReceiverId}");
                    result.Successful = false;
                    result.ErrorMessage = "Can't find the Budget invite to respond.";
                    return result;
                }

                Budget? budget = await _UnitOfWork.BudgetRepository.GetBudgetById(budgetInvite.BudgetId, budgetInvite.SenderId);

                if (budget == null)
                {
                    Console.WriteLine($"Can't find budget with id {budgetInvite.BudgetId} to create invite.");
                    result.Successful = false;
                    result.ErrorMessage = "Can't find the Budget invite to respond.";
                    return result;
                }

                budgetInvite.Accepted = accepted;

                if (accepted)
                {
                    budget.Users.Add(budgetInvite.Receiver);
                }

                BudgetInvite createdInvite = _UnitOfWork.BudgetInviteRepository.UpdateInvite(budgetInvite);

                await _UnitOfWork.CommitAsync();
                await SendBudgetInviteNotification(budgetInvite.Sender.NotificationToken, budgetInvite.Accepted.Value);

                result.Data = createdInvite.ToDto();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                result.Successful = false;
                result.ErrorMessage = "Something got wrong during respond to Invite. Please try again later.";
            }

            return result;
        }

        private async Task SendBudgetInviteNotification(string receiverNotificationToken, bool accepted)
        {
            string notificationMessage = accepted
                ? "The invite for a budget you sent is accepted by the receiver!"
                : "The invite for a budget you sent is declined by the receiver!";
            await _PushNotificationsService.SendNotification([receiverNotificationToken], "Budget Invite Status", notificationMessage, new NotificationDto() { Type = NotificationTypeEnum.InviteResponse });
        }
    }
}
