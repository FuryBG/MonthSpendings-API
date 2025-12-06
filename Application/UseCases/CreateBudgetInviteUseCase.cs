using Application.Contracts;
using Application.Dto;
using Application.Interfaces;
using Application.Mappers;
using Application.Services;
using Domain;

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
        private IPushNotificationsService _PushNotificationsService { get; set; }
        public CreateBudgetInviteUseCase(IUnitOfWork unitOfWork, IUserService userService, IPushNotificationsService pushNotificationService)
        {
            _UnitOfWork = unitOfWork;
            _UserService = userService;
            _PushNotificationsService = pushNotificationService;
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
                    Console.WriteLine($"Can't find budget with id {budgetInviteDto.BudgetId} to create invite.");
                    result.Successful = false;
                    result.ErrorMessage = "Can't find the Budget to send Invite.";
                    return result;
                }

                AppUser? sender = await _UnitOfWork.UserRepository.GetUserById(userId);

                if (sender == null)
                {
                    Console.WriteLine($"Can't find sender user with id {userId} to create invite.");
                    result.Successful = false;
                    result.ErrorMessage = "Corrupted user, please log in again, and send invite again.";
                    return result;
                }

                AppUser? receiver = await _UnitOfWork.UserRepository.GetUserByEmail(budgetInviteDto.ReceiverEmail);

                if (receiver == null || receiver.NotificationToken == null || receiver.NotificationToken == string.Empty)
                {
                    Console.WriteLine($"Can't find receiver user with id {userId} to create invite.");
                    result.Successful = false;
                    result.ErrorMessage = "User with this email doesn't exist.";
                    return result;
                }

                BudgetInvite budgetInvite = budgetInviteDto.ToEntity();
                budgetInvite.ReceiverId = receiver.Id;

                BudgetInvite createdInvite = _UnitOfWork.BudgetInviteRepository.CreateInvite(budgetInvite);

                await _UnitOfWork.CommitAsync();
                await SendBudgetInviteNotification(receiver.NotificationToken);

                result.Data = createdInvite.ToDto();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                result.Successful = false;
                result.ErrorMessage = "Something got wrong during creating Invite. Please try again later.";
            }

            return result;
        }

        private async Task SendBudgetInviteNotification(string receiverNotificationToken)
        {
            await _PushNotificationsService.SendNotification([receiverNotificationToken], "Budget Invite", "You have been invited for a Budget. Click to see the invite.");
        }
    }
}
