using Application.Dto.Bank;
using Domain.Bank;

namespace Application.Mappers
{
    public static class BankAccountMapper
    {
        public static BankAccountDto ToDto(this BankAccount bankAccount)
        {
            return new BankAccountDto()
            {
                Id = bankAccount.Id,
                Iban = bankAccount.Iban,
                Currency = bankAccount.Currency,
                HolderName = bankAccount.HolderName,
            };
        }
    }
}