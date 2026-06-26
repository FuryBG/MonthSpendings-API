using Application.Dto;
using Domain;

namespace Application.Mappers
{
    public static class AppUserMapper
    {
        public static AppUserDto ToDto(this AppUser appUser)
        {
            return new AppUserDto()
            {
                Id = appUser.Id,
                GoogleId = appUser.GoogleId,
                GooglePhotoAddress = appUser.GooglePhotoAddress,
                Email = appUser.Email,
                FirstName = appUser.FirstName,
                LastName = appUser.LastName,
                NotificationToken = appUser.NotificationToken,
                IsPro = appUser.IsPro,
                Subscription = appUser.Subscriptions.MaxBy(s => s.CreatedAt)?.ToDto(),
                ReceivedBudgetInvites = appUser.ReceivedBudgetInvites.Select(invite => invite.ToDto()).ToList(),
                SentBudgetInvites = appUser.SentBudgetInvites.Select(invite => invite.ToDto()).ToList()
            };
        }

        public static AppUser ToEntity(this AppUserDto dto)
        {
            return new AppUser()
            {
                Id = dto.Id,
                GoogleId = dto.GoogleId,
                GooglePhotoAddress = dto.GooglePhotoAddress,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                NotificationToken = dto.NotificationToken
            };
        }
    }
}
