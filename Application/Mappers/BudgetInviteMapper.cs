using Application.Dto;
using Domain;

namespace Application.Mappers
{
    public static class BudgetInviteMapper
    {
        public static BudgetInviteDto ToDto(this BudgetInvite budgetInvite)
        {
            return new BudgetInviteDto()
            {
                Id = budgetInvite.Id,
                ReceiverEmail = budgetInvite.Receiver.Email,
                BudgetId = budgetInvite.BudgetId,
                Accepted = budgetInvite.Accepted,
                ValidTo = budgetInvite.ValidTo,
                BudgetName = budgetInvite.Budget?.Name ?? string.Empty,
                SenderEmail = budgetInvite.Sender?.Email ?? string.Empty,
                SenderName = $"{budgetInvite.Sender?.FirstName} {budgetInvite.Sender?.LastName}".Trim()
            };
        }

        public static BudgetInvite ToEntity(this BudgetInviteDto dto)
        {
            return new BudgetInvite()
            {
                Id = dto.Id,
                BudgetId = dto.BudgetId,
                Accepted = dto.Accepted,
            };
        }
    }
}
