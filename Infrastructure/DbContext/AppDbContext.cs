using Domain;
using Domain.Bank;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure
{
    public class AppDbContext : DbContext
    {
        public DbSet<AppUser> Users { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<Budget> Budgets { get; set; }
        public DbSet<Currency> Currencies { get; set; }
        public DbSet<BudgetPeriod> BudgetPeriods { get; set; }
        public DbSet<BudgetCategory> BudgetCategories { get; set; }
        public DbSet<BudgetInvite> BudgetInvites { get; set; }
        public DbSet<Spending> Spendings { get; set; }
        public DbSet<NotificationTransaction> NotificationTransactions { get; set; }
        public DbSet<TransactionCategoryRule> TransactionCategoryRules { get; set; }
        public DbSet<AccountDeleteRequest> AccountDeleteRequests { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AppUser>()
                .HasMany(u => u.Subscriptions)
                .WithOne(s => s.User)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BudgetCategory>()
                .HasQueryFilter(c => !c.IsDeleted);

            modelBuilder.Entity<Budget>()
                .HasQueryFilter(b => !b.IsDeleted);

            modelBuilder.Entity<NotificationTransaction>()
                .HasQueryFilter(t => !t.IsDeleted);

            modelBuilder.Entity<NotificationTransaction>()
                .HasOne(t => t.Spending)
                .WithOne(s => s.NotificationTransaction)
                .HasForeignKey<NotificationTransaction>(t => t.SpendingId)
                .IsRequired(false);

            modelBuilder.Entity<NotificationTransaction>()
                .HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TransactionCategoryRule>()
                .HasOne(r => r.Category)
                .WithMany()
                .HasForeignKey(r => r.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

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

            modelBuilder.Entity<Spending>()
                .HasOne(s => s.CreatedBy)
                .WithMany(u => u.CreatedSpendings)
                .HasForeignKey(s => s.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RefreshToken>()
                .HasOne(rt => rt.User)
                .WithMany()
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RefreshToken>()
                .HasIndex(rt => rt.Token)
                .IsUnique();

            modelBuilder.Entity<RefreshToken>()
                .HasIndex(rt => rt.UserId);

            modelBuilder.Entity<Currency>().HasData(
                new Currency { Id = 1, Code = "USD", Name = "US Dollar", Symbol = "$" },
                new Currency { Id = 2, Code = "EUR", Name = "Euro", Symbol = "€" },
                new Currency { Id = 3, Code = "GBP", Name = "British Pound", Symbol = "£" },
                new Currency { Id = 4, Code = "JPY", Name = "Japanese Yen", Symbol = "¥" },
                new Currency { Id = 5, Code = "CAD", Name = "Canadian Dollar", Symbol = "C$" },
                new Currency { Id = 6, Code = "AUD", Name = "Australian Dollar", Symbol = "A$" },
                new Currency { Id = 7, Code = "CHF", Name = "Swiss Franc", Symbol = "CHF" },
                new Currency { Id = 8, Code = "CNY", Name = "Chinese Yuan Renminbi", Symbol = "¥" },
                new Currency { Id = 9, Code = "SEK", Name = "Swedish Krona", Symbol = "kr" },
                new Currency { Id = 10, Code = "NOK", Name = "Norwegian Krone", Symbol = "kr" },
                new Currency { Id = 11, Code = "DKK", Name = "Danish Krone", Symbol = "kr" },
                new Currency { Id = 12, Code = "NZD", Name = "New Zealand Dollar", Symbol = "NZ$" },
                new Currency { Id = 13, Code = "SGD", Name = "Singapore Dollar", Symbol = "S$" },
                new Currency { Id = 14, Code = "HKD", Name = "Hong Kong Dollar", Symbol = "HK$" },
                new Currency { Id = 15, Code = "KRW", Name = "South Korean Won", Symbol = "₩" },
                new Currency { Id = 16, Code = "INR", Name = "Indian Rupee", Symbol = "₹" },
                new Currency { Id = 17, Code = "MXN", Name = "Mexican Peso", Symbol = "$" },
                new Currency { Id = 18, Code = "BRL", Name = "Brazilian Real", Symbol = "R$" },
                new Currency { Id = 19, Code = "RUB", Name = "Russian Ruble", Symbol = "₽" },
                new Currency { Id = 20, Code = "ZAR", Name = "South African Rand", Symbol = "R" },
                new Currency { Id = 21, Code = "TRY", Name = "Turkish Lira", Symbol = "₺" },
                new Currency { Id = 22, Code = "AED", Name = "UAE Dirham", Symbol = "د.إ" },
                new Currency { Id = 23, Code = "PLN", Name = "Polish Zloty", Symbol = "zł" },
                new Currency { Id = 24, Code = "THB", Name = "Thai Baht", Symbol = "฿" },
                new Currency { Id = 25, Code = "IDR", Name = "Indonesian Rupiah", Symbol = "Rp" },
                new Currency { Id = 26, Code = "MYR", Name = "Malaysian Ringgit", Symbol = "RM" },
                new Currency { Id = 27, Code = "PHP", Name = "Philippine Peso", Symbol = "₱" },
                new Currency { Id = 28, Code = "HUF", Name = "Hungarian Forint", Symbol = "Ft" },
                new Currency { Id = 29, Code = "CZK", Name = "Czech Koruna", Symbol = "Kč" },
                new Currency { Id = 30, Code = "ILS", Name = "Israeli Shekel", Symbol = "₪" },
                new Currency { Id = 31, Code = "CLP", Name = "Chilean Peso", Symbol = "$" },
                new Currency { Id = 32, Code = "PKR", Name = "Pakistani Rupee", Symbol = "₨" },
                new Currency { Id = 33, Code = "EGP", Name = "Egyptian Pound", Symbol = "£" },
                new Currency { Id = 34, Code = "SAR", Name = "Saudi Riyal", Symbol = "﷼" },
                new Currency { Id = 35, Code = "COP", Name = "Colombian Peso", Symbol = "$" },
                new Currency { Id = 36, Code = "VND", Name = "Vietnamese Dong", Symbol = "₫" },
                new Currency { Id = 37, Code = "BDT", Name = "Bangladeshi Taka", Symbol = "৳" },
                new Currency { Id = 38, Code = "NGN", Name = "Nigerian Naira", Symbol = "₦" },
                new Currency { Id = 39, Code = "KWD", Name = "Kuwaiti Dinar", Symbol = "د.ك" },
                new Currency { Id = 40, Code = "QAR", Name = "Qatari Riyal", Symbol = "﷼" },
                new Currency { Id = 41, Code = "OMR", Name = "Omani Rial", Symbol = "﷼" },
                new Currency { Id = 42, Code = "TWD", Name = "New Taiwan Dollar", Symbol = "NT$" },
                new Currency { Id = 43, Code = "ARS", Name = "Argentine Peso", Symbol = "$" },
                new Currency { Id = 44, Code = "UAH", Name = "Ukrainian Hryvnia", Symbol = "₴" }
            );
        }
    }
}
