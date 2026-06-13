using Application.Interfaces;
using Application.Interfaces.Repository;
using Application.Interfaces.Repository.Bank;
using Application.Interfaces.Repository.SaltEdge;
using Microsoft.EntityFrameworkCore.Storage;

namespace Infrastructure
{
    public class UnitOfWork : IUnitOfWork, IAsyncDisposable
    {
        private AppDbContext _DbContext { get; set; }
        private IDbContextTransaction? _CurrentTransaction;
        public IUserRepository UserRepository { get; private set; }
        public IBudgetRepository BudgetRepository { get; private set; }
        public IBudgetCategoryRepository BudgetCategoryRepository { get; private set; }
        public ICategorySpendingsRepository CategorySpendingsRepository { get; private set; }
        public IBudgetInviteRepository BudgetInviteRepository { get; private set; }
        public ICurrencyRepository CurrencyRepository { get; private set; }
        public IBankConsentRepository BankConsentRepository { get; private set; }
        public IBankAccountRepository BankAccountRepository { get; private set; }
        public IBankTransactionRepository BankTransactionRepository { get; private set; }
        public ITransactionCategoryRuleRepository TransactionCategoryRuleRepository { get; private set; }
        public ISaltEdgeCustomerRepository SaltEdgeCustomerRepository { get; private set; }
        public ISaltEdgeConnectionRepository SaltEdgeConnectionRepository { get; private set; }
        public ISaltEdgeAccountRepository SaltEdgeAccountRepository { get; private set; }
        public ISaltEdgeTransactionRepository SaltEdgeTransactionRepository { get; private set; }
        public IStatisticsRepository StatisticsRepository { get; private set; }

        public UnitOfWork(
            AppDbContext dbContext,
            IUserRepository userRepository,
            IBudgetRepository budgetRepository,
            IBudgetCategoryRepository budgetCategoryRepository,
            ICategorySpendingsRepository categorySpendingsRepository,
            IBudgetInviteRepository budgetInviteRepository,
            ICurrencyRepository currencyRepository,
            IBankConsentRepository bankConsentRepository,
            IBankAccountRepository bankAccountRepository,
            IBankTransactionRepository bankTransactionRepository,
            ITransactionCategoryRuleRepository transactionCategoryRuleRepository,
            ISaltEdgeCustomerRepository saltEdgeCustomerRepository,
            ISaltEdgeConnectionRepository saltEdgeConnectionRepository,
            ISaltEdgeAccountRepository saltEdgeAccountRepository,
            ISaltEdgeTransactionRepository saltEdgeTransactionRepository,
            IStatisticsRepository statisticsRepository)
        {
            _DbContext = dbContext;
            UserRepository = userRepository;
            BudgetRepository = budgetRepository;
            BudgetCategoryRepository = budgetCategoryRepository;
            CategorySpendingsRepository = categorySpendingsRepository;
            BudgetInviteRepository = budgetInviteRepository;
            CurrencyRepository = currencyRepository;
            BankConsentRepository = bankConsentRepository;
            BankAccountRepository = bankAccountRepository;
            BankTransactionRepository = bankTransactionRepository;
            TransactionCategoryRuleRepository = transactionCategoryRuleRepository;
            SaltEdgeCustomerRepository = saltEdgeCustomerRepository;
            SaltEdgeConnectionRepository = saltEdgeConnectionRepository;
            SaltEdgeAccountRepository = saltEdgeAccountRepository;
            SaltEdgeTransactionRepository = saltEdgeTransactionRepository;
            StatisticsRepository = statisticsRepository;
        }

        public async Task CommitAsync()
        {
            await _DbContext.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            if (_CurrentTransaction != null)
            {
                return;
            }

            _CurrentTransaction = await _DbContext.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_CurrentTransaction == null)
            {
                throw new InvalidOperationException("No transaction in progress");
            }

            try
            {
                await _DbContext.SaveChangesAsync();
                await _CurrentTransaction.CommitAsync();
            }
            finally
            {
                await DisposeTransactionAsync();
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_CurrentTransaction == null)
            {
                throw new InvalidOperationException("No transaction in progress");
            }

            try
            {
                await _CurrentTransaction.RollbackAsync();
            }
            finally
            {
                await DisposeTransactionAsync();
            }
        }

        private async Task DisposeTransactionAsync()
        {
            if (_CurrentTransaction != null)
            {
                await _CurrentTransaction.DisposeAsync();
                _CurrentTransaction = null;
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_CurrentTransaction != null)
            {
                await _CurrentTransaction.RollbackAsync();
                await _CurrentTransaction.DisposeAsync();
                _CurrentTransaction = null;
            }
        }
    }
}
