using Application.Interfaces.Repository;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repository
{
    public class SavingsPotInviteRepository : ISavingsPotInviteRepository
    {
        private AppDbContext _DbContext { get; set; }
        public SavingsPotInviteRepository(AppDbContext dbContext)
        {
            _DbContext = dbContext;
        }

        public async Task<SavingsPotInvite?> GetById(int inviteId)
        {
            return await _DbContext.SavingsPotInvites
                .Include(i => i.SavingsPot)
                .ThenInclude(sp => sp.Users)
                .Include(i => i.Sender)
                .Include(i => i.Receiver)
                .FirstOrDefaultAsync(i => i.Id == inviteId);
        }

        public SavingsPotInvite Create(SavingsPotInvite invite)
        {
            _DbContext.SavingsPotInvites.Add(invite);
            return invite;
        }

        public SavingsPotInvite Update(SavingsPotInvite invite)
        {
            _DbContext.SavingsPotInvites.Update(invite);
            return invite;
        }
    }
}
