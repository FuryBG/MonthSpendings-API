namespace Application.Dto
{
    public class BudgetInviteDto
    {
        public int Id { get; set; }
        public int SenderId { get; set; }
        public required string ReceiverEmail { get; set; }
        public int BudgetId { get; set; }
        public DateTime ValidTo { get; set; }
        public bool? Accepted { get; set; }
    }
}
