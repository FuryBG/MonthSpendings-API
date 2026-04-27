using Application.Dto.Savings;
using Domain;

namespace Application.Mappers
{
    public static class SavingsPotMapper
    {
        public static SavingsPotDto ToDto(this SavingsPot pot)
        {
            return new SavingsPotDto
            {
                Id = pot.Id,
                Name = pot.Name,
                Currency = pot.Currency.ToDto(),
                TotalSaved = pot.Contributions.Sum(c => c.Amount),
                CreatedByUserId = pot.CreatedByUserId,
                CreatedAt = pot.CreatedAt,
                Users = pot.Users.Select(u => u.ToDto()).ToList(),
                RecentContributions = pot.Contributions
                    .OrderByDescending(c => c.Date)
                    .Take(5)
                    .Select(c => c.ToDto())
                    .ToList(),
            };
        }

        public static SavingsContributionDto ToDto(this SavingsContribution contribution)
        {
            return new SavingsContributionDto
            {
                Id = contribution.Id,
                SavingsPotId = contribution.SavingsPotId,
                Amount = contribution.Amount,
                Date = contribution.Date,
                Note = contribution.Note,
                AddedByUserId = contribution.AddedByUserId,
                AddedByName = contribution.AddedBy != null
                    ? $"{contribution.AddedBy.FirstName} {contribution.AddedBy.LastName}".Trim()
                    : null,
                AddedByEmail = contribution.AddedBy?.Email,
            };
        }

        public static SavingsPotInviteDto ToDto(this SavingsPotInvite invite)
        {
            return new SavingsPotInviteDto
            {
                Id = invite.Id,
                SavingsPotId = invite.SavingsPotId,
                ReceiverEmail = invite.Receiver?.Email ?? "",
                ValidTo = invite.ValidTo,
                Accepted = invite.Accepted,
            };
        }
    }
}
