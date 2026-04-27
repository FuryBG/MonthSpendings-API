using Application.Interfaces.Repository;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository
{
    public class SavingsPotRepository : ISavingsPotRepository
    {
        private AppDbContext _DbContext { get; set; }
        public SavingsPotRepository(AppDbContext dbContext)
        {
            _DbContext = dbContext;
        }

        public async Task<SavingsPot?> GetByIdForUser(int savingsPotId, int userId)
        {
            return await _DbContext.SavingsPots
                .Include(sp => sp.Currency)
                .Include(sp => sp.Users)
                .Include(sp => sp.Contributions.OrderByDescending(c => c.Date).Take(5))
                .ThenInclude(c => c.AddedBy)
                .Where(sp => sp.Id == savingsPotId && sp.Users.Any(u => u.Id == userId))
                .FirstOrDefaultAsync();
        }

        public async Task<List<SavingsPot>> GetAllForUser(int userId)
        {
            return await _DbContext.SavingsPots
                .Include(sp => sp.Currency)
                .Include(sp => sp.Users)
                .Include(sp => sp.Contributions.OrderByDescending(c => c.Date).Take(5))
                .ThenInclude(c => c.AddedBy)
                .Where(sp => sp.Users.Any(u => u.Id == userId))
                .ToListAsync();
        }

        public SavingsPot Create(SavingsPot savingsPot)
        {
            _DbContext.Attach(savingsPot.Currency);
            _DbContext.SavingsPots.Add(savingsPot);
            return savingsPot;
        }

        public SavingsPot Delete(SavingsPot savingsPot)
        {
            _DbContext.SavingsPots.Remove(savingsPot);
            return savingsPot;
        }

        public async Task<SavingsContribution?> GetContributionById(int contributionId)
        {
            return await _DbContext.SavingsContributions
                .Include(c => c.SavingsPot)
                .ThenInclude(sp => sp.Users)
                .FirstOrDefaultAsync(c => c.Id == contributionId);
        }

        public SavingsContribution AddContribution(SavingsContribution contribution)
        {
            _DbContext.SavingsContributions.Add(contribution);
            return contribution;
        }

        public SavingsContribution RemoveContribution(SavingsContribution contribution)
        {
            _DbContext.SavingsContributions.Remove(contribution);
            return contribution;
        }

        public async Task<List<SavingsContribution>> GetContributions(int savingsPotId)
        {
            return await _DbContext.SavingsContributions
                .Include(c => c.AddedBy)
                .Where(c => c.SavingsPotId == savingsPotId)
                .OrderByDescending(c => c.Date)
                .ToListAsync();
        }
    }
}
