using Domain;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure
{
    public class AppDbContext : DbContext
    {
        public DbSet<AppUser> Users { get; set; }
        public DbSet<Budget> Budgets { get; set; }
        public DbSet<BudgetPeriod> BudgetPeriods { get; set; }
        public DbSet<BudgetCategory> BudgetCategories { get; set; }
        public DbSet<BudgetInvite> BudgetInvites { get; set; }
        public DbSet<Spending> Spendings { get; set; }
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<BudgetPeriod>()
            .Property(i => i.StartDate)
            .HasDefaultValueSql("timezone('utc', now())");

            modelBuilder.Entity<BudgetInvite>()
           .Property(i => i.ValidTo)
           .HasDefaultValueSql("timezone('utc', now()) + INTERVAL '2 days'");

            modelBuilder.Entity<BudgetInvite>()
            .HasOne(bi => bi.Sender)
            .WithMany(u => u.SentBudgetInvites)
            .HasForeignKey(bi => bi.SenderId)
            .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BudgetInvite>()
            .HasOne(bi => bi.Receiver)
            .WithMany(u => u.ReceivedBudgetInvites)
            .HasForeignKey(bi => bi.ReceiverId)
            .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
