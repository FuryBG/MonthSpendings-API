using Application.Dto.Bank;
using Domain.Bank;

namespace Application.Mappers
{
    public static class BankTransactionMapper
    {
        public static BankTransactionDto ToDto(this BankTransaction transaction)
        {
            return new BankTransactionDto
            {
                Id = transaction.Id,
                TransactionId = transaction.TransactionId,
                BankAccountId = transaction.BankAccountId,
                Currency = transaction.Currency,
                Amount = transaction.Amount,
                MerchantCode = transaction.MerchantCode,
                CreditorName = transaction.CreditorName,
                Description = transaction.Description,
                Status = transaction.Status.ToString(),
                BookingDate = transaction.BookingDate,
                Categorized = transaction.Categorized,
                Determined = transaction.Determined,
            };
        }
    }
}
