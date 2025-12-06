using Application.Interfaces.Repository;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository
{
    public class BudgetInviteRepository : IBudgetInviteRepository
    {
        private AppDbContext _DbContext { get; set; }
        public BudgetInviteRepository(AppDbContext dbContext)
        {
            _DbContext = dbContext;
        }

        public async Task<List<BudgetInvite>> GetBudgetInvites(int userId)
        {
            return await _DbContext.BudgetInvites
                .Where(bi => bi.SenderId == userId || bi.ReceiverId == userId)
                .ToListAsync();
        }

        public BudgetInvite CreateInvite(BudgetInvite budgetInvite)
        {
            _DbContext.BudgetInvites.Add(budgetInvite);
            return budgetInvite;
        }

        public BudgetInvite UpdateInvite(BudgetInvite budgetInvite)
        {
            _DbContext.BudgetInvites.Update(budgetInvite);
            return budgetInvite;
        }

        public BudgetInvite DeleteInvite(BudgetInvite budgetInvite)
        {
            _DbContext.BudgetInvites.Remove(budgetInvite);
            return budgetInvite;
        }
    }
}
