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
                SenderId = budgetInvite.SenderId,
                ReceiverEmail = budgetInvite.Receiver.Email,
                BudgetId = budgetInvite.BudgetId,
                Accepted = budgetInvite.Accepted,
                ValidTo = budgetInvite.ValidTo
            };
        }

        public static BudgetInvite ToEntity(this BudgetInviteDto dto)
        {
            return new BudgetInvite()
            {
                Id = dto.Id,
                SenderId = dto.SenderId,
                BudgetId = dto.BudgetId,
                Accepted = dto.Accepted,
                ValidTo = dto.ValidTo
            };
        }
    }
}
