
using System.ComponentModel.DataAnnotations;

namespace Domain
{
    public class AppUser
    {
        [Key]
        public int Id { get; set; }
        public required string Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string NotificationToken { get; set; } = "";
        public string GoogleId { get; set; } = "";
        public string? GooglePhotoAddress { get; set; }
        public List<Budget> Budgets { get; set; } = [];
        public List<BudgetInvite> SentBudgetInvites { get; set; } = new();
        public List<BudgetInvite> ReceivedBudgetInvites { get; set; } = new();
    }
}
