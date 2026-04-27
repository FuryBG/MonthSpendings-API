using Application.Contracts;
using Application.Interfaces;
using Application.Services;
using Domain;

namespace Application.UseCases.Savings
{
    public interface IUpdateSavingsPotInviteResponseUseCase
    {
        Task<CaseResult<bool>> InvokeAsync(int inviteId, bool accepted);
    }

    public class UpdateSavingsPotInviteResponseUseCase : IUpdateSavingsPotInviteResponseUseCase
    {
        private IUnitOfWork _UnitOfWork { get; set; }
        private IUserService _UserService { get; set; }
        public UpdateSavingsPotInviteResponseUseCase(IUnitOfWork unitOfWork, IUserService userService)
        {
            _UnitOfWork = unitOfWork;
            _UserService = userService;
        }

        public async Task<CaseResult<bool>> InvokeAsync(int inviteId, bool accepted)
        {
            var result = new CaseResult<bool>();
            result.Successful = true;

            try
            {
                int userId = _UserService.GetUserId();
                var invite = await _UnitOfWork.SavingsPotInviteRepository.GetById(inviteId);

                if (invite == null || invite.ReceiverId != userId)
                {
                    result.Successful = false;
                    result.ErrorMessage = "Invite not found.";
                    return result;
                }

                invite.Accepted = accepted;
                _UnitOfWork.SavingsPotInviteRepository.Update(invite);

                if (accepted)
                {
                    var pot = await _UnitOfWork.SavingsPotRepository.GetByIdForUser(invite.SavingsPotId, invite.SenderId);
                    if (pot != null)
                    {
                        AppUser? receiver = await _UnitOfWork.UserRepository.GetUserById(userId);
                        if (receiver != null && !pot.Users.Any(u => u.Id == userId))
                        {
                            pot.Users.Add(receiver);
                        }
                    }
                }

                await _UnitOfWork.CommitAsync();
                result.Data = accepted;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                result.Successful = false;
                result.ErrorMessage = "Something went wrong while responding to the invite.";
            }

            return result;
        }
    }
}
