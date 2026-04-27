using Application.Contracts;
using Application.Dto.Savings;
using Application.Dto.Notification;
using Application.Enums;
using Application.Interfaces;
using Application.Mappers;
using Application.Services;
using Domain;

namespace Application.UseCases.Savings
{
    public interface ISendSavingsPotInviteUseCase
    {
        Task<CaseResult<SavingsPotInviteDto?>> InvokeAsync(SavingsPotInviteDto dto);
    }

    public class SendSavingsPotInviteUseCase : ISendSavingsPotInviteUseCase
    {
        private IUnitOfWork _UnitOfWork { get; set; }
        private IUserService _UserService { get; set; }
        private IPushNotificationService _PushNotificationService { get; set; }
        public SendSavingsPotInviteUseCase(IUnitOfWork unitOfWork, IUserService userService, IPushNotificationService pushNotificationService)
        {
            _UnitOfWork = unitOfWork;
            _UserService = userService;
            _PushNotificationService = pushNotificationService;
        }

        public async Task<CaseResult<SavingsPotInviteDto?>> InvokeAsync(SavingsPotInviteDto dto)
        {
            var result = new CaseResult<SavingsPotInviteDto?>();
            result.Successful = true;

            try
            {
                int userId = _UserService.GetUserId();
                var pot = await _UnitOfWork.SavingsPotRepository.GetByIdForUser(dto.SavingsPotId, userId);

                if (pot == null)
                {
                    result.Successful = false;
                    result.ErrorMessage = "Savings pot not found.";
                    return result;
                }

                AppUser? receiver = await _UnitOfWork.UserRepository.GetUserByEmail(dto.ReceiverEmail);
                if (receiver == null || string.IsNullOrEmpty(receiver.NotificationToken))
                {
                    result.Successful = false;
                    result.ErrorMessage = "User with this email doesn't exist.";
                    return result;
                }

                var invite = new SavingsPotInvite
                {
                    SavingsPotId = dto.SavingsPotId,
                    SenderId = userId,
                    ReceiverId = receiver.Id,
                    ValidTo = DateTime.UtcNow.AddDays(2),
                };

                var created = _UnitOfWork.SavingsPotInviteRepository.Create(invite);
                await _UnitOfWork.CommitAsync();

                await _PushNotificationService.SendNotification(
                    [receiver.NotificationToken],
                    "Savings Pot Invite",
                    $"You've been invited to join a savings pot.",
                    new NotificationDto { Type = NotificationTypeEnum.ReceivedInvite });

                var loaded = await _UnitOfWork.SavingsPotInviteRepository.GetById(created.Id);
                result.Data = loaded?.ToDto() ?? created.ToDto();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                result.Successful = false;
                result.ErrorMessage = "Something went wrong while sending the invite.";
            }

            return result;
        }
    }
}
