using System.ComponentModel.DataAnnotations;

namespace Domain
{
    public class Budget
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public MonthlyBudget MonthlyBudget { get; set; }
        public List<AppUser> Users { get; set; }
    }
}
