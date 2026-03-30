using Application.BackgroundWorkers;
using Application.Interfaces;
using Application.Interfaces.Repository;
using Application.Interfaces.Repository.Bank;
using Application.Services;
using Application.UseCases;
using Application.UseCases.Bank;
using EnableBanking;
using Infrastructure;
using Infrastructure.Repository;
using Infrastructure.Repository.Bank;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using MonthSpendings.BackgroundServices;
using System.Text;

namespace MonthSpendings;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("AppDb"), na => na.MigrationsAssembly("Infrastructure")));

        builder.Services.AddControllers();
        builder.Services.Configure<ApiBehaviorOptions>(options =>
        {
            options.SuppressModelStateInvalidFilter = true;
        });

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        //builder.Services.AddOpenApi();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Paste JWT token here"
            });
        });

        builder.Services.AddHttpContextAccessor();

        builder.Services.AddTransient<ITokenService, TokenService>();

        builder.Services.AddScoped<IUserService, UserService>();

        builder.Services.AddTransient<IUnitOfWork, UnitOfWork>();
        builder.Services.AddTransient<IUserRepository, UserRepository>();
        builder.Services.AddTransient<IBudgetRepository, BudgetRepository>();
        builder.Services.AddTransient<IBudgetCategoryRepository, BudgetCategoryRepository>();
        builder.Services.AddTransient<ICategorySpendingsRepository, CategorySpendingsRepository>();
        builder.Services.AddTransient<IBudgetInviteRepository, BudgetInviteRepository>();
        builder.Services.AddTransient<ICurrencyRepository, CurrencyRepository>();

        builder.Services.AddTransient<IRegisterUserUseCase, RegisterUserUseCase>();
        builder.Services.AddTransient<IGetUserByIdUseCase, GetUserByIdUseCase>();

        builder.Services.AddTransient<ICreateBudgetUseCase, CreateBudgetUseCase>();
        builder.Services.AddTransient<IGetAllBudgetsUseCase, GetAllBudgetsUseCase>();
        builder.Services.AddTransient<IDeleteBudgetUseCase, DeleteBudgetUseCase>();
        builder.Services.AddTransient<IFinishBudgetPeriodUseCase, FinishBudgetPeriodUseCase>();

        builder.Services.AddTransient<ICreateBudgetCategoryUseCase, CreateBudgetCategoryUseCase>();
        builder.Services.AddTransient<IDeleteBudgetCategoryUseCase, DeleteBudgetCategoryUseCase>();

        builder.Services.AddTransient<ICreateSpendingUseCase, CreateSpendingUseCase>();
        builder.Services.AddTransient<IDeleteSpendingUseCase, DeleteSpendingUseCase>();

        builder.Services.AddTransient<ICreateBudgetInviteUseCase, CreateBudgetInviteUseCase>();
        builder.Services.AddTransient<IUpdateBudgetInviteResponseUseCase, UpdateBudgetInviteResponseUseCase>();

        builder.Services.AddTransient<IGetAllCurrenciesUseCase, GetAllCurrenciesUseCase>();

        // INTEGRATIONS
        string? bankingCertificatePath = builder.Configuration.GetSection("EnableBanking:AppCertPath").Value;
        string? bankingAppKeyId = builder.Configuration.GetSection("EnableBanking:AppKeyId").Value;

        if (string.IsNullOrWhiteSpace(bankingCertificatePath))
        {
            throw new InvalidOperationException("EnableBanking:AppCertPath is not configured in appsettings.json or environment variables.");
        }

        if (string.IsNullOrWhiteSpace(bankingAppKeyId))
        {
            throw new InvalidOperationException("EnableBanking:AppKeyId is not configured in appsettings.json or environment variables.");
        }
        builder.Services.AddTransient<IPushNotificationService, PushNotificationsService>();

        builder.Services.AddEnableBankingApi(options =>
        {
            options.KeyPath = Path.Combine(builder.Environment.ContentRootPath, bankingCertificatePath);
            options.AppKid = bankingAppKeyId;
        });

        builder.Services.AddTransient<IGetBanksUseCase, GetBanksUseCase>();
        builder.Services.AddTransient<IStartBankConnectionUseCase, StartBankConnectionUseCase>();
        builder.Services.AddTransient<IFinishBankConnectionUseCase, FinishBankConnectionUseCase>();

        builder.Services.AddTransient<IBankConsentRepository, BankConsentRepository>();
        builder.Services.AddTransient<IBankAccountRepository, BankAccountRepository>();
        builder.Services.AddTransient<IBankTransactionRepository, BankTransactionRepository>();

        builder.Services.AddTransient<ICategorizeTransactionsUseCase, CategorizeTransactionsUseCase>();
        builder.Services.AddTransient<IGetUncategorizedTransactionsUseCase, GetUncategorizedTransactionsUseCase>();


        builder.Services.AddTransient<IBankSyncWorker, BankSyncWorker>();

        builder.Services.AddHostedService<TransactionSyncBackgroundService>();

        // END INTEGRATIONS

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(jwtOptions =>
        {
            jwtOptions.RequireHttpsMetadata = false;
            jwtOptions.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
                )
            };
        });


        var app = builder.Build();

        // Configure the HTTP request pipeline.
        //if (app.Environment.IsDevelopment())
        //{
        //    app.MapOpenApi();
        //}

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        //app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapSwagger();
        app.MapControllers();

        // --- Database Migration Logic ---
        using (var scope = app.Services.CreateScope())
        {
            var services = scope.ServiceProvider;
            try
            {
                var context = services.GetRequiredService<AppDbContext>();
                Console.WriteLine("Applying migrations...");
                context.Database.Migrate();
                Console.WriteLine("Migrations applied successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred during migration: {ex.Message}");
            }
        }
        app.Run();
    }
}
