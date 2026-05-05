using Application.BackgroundWorkers;
using Application.Interfaces;
using Application.Interfaces.Repository;
using Application.Interfaces.Repository.Bank;
using Application.Interfaces.Repository.SaltEdge;
using Application.Services;
using Application.UseCases;
using Application.UseCases.Bank;
using Application.UseCases.SaltEdge;
using Application.UseCases.Savings;
using Application.UseCases.Statistics;
using EnableBanking;
using Infrastructure;
using Infrastructure.Interceptors;
using Infrastructure.Repository;
using Infrastructure.Repository.Bank;
using Infrastructure.Repository.SaltEdge;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using MonthSpendings.BackgroundServices;
using MonthSpendings.Exceptions;
using MonthSpendings.Middleware;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using SaltEdge;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using System.Text;

namespace MonthSpendings;

public class Program
{
    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateBootstrapLogger();

        try
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddSerilog((services, lc) => lc
                .ReadFrom.Configuration(builder.Configuration)
                .ReadFrom.Services(services)
                .Enrich.WithExceptionDetails()
                .WriteTo.Async(a => a.PostgreSQL(
                    connectionString: builder.Configuration.GetConnectionString("AppDb") ?? throw new Exception("Connection string AppDb is null."),
                    tableName: "Logs",
                    columnOptions: null,
                    needAutoCreateTable: true,
                    restrictedToMinimumLevel: builder.Environment.IsDevelopment()
                        ? LogEventLevel.Information
                        : LogEventLevel.Warning)));

            builder.Services.AddOpenTelemetry()
                .ConfigureResource(r => r.AddService("MonthSpendings-API"))
                .WithTracing(tracing => tracing
                    .AddAspNetCoreInstrumentation(opts => opts.RecordException = true)
                    .AddEntityFrameworkCoreInstrumentation(opts => opts.SetDbStatementForText = true)
                    .AddHttpClientInstrumentation()
                    .AddConsoleExporter()
                    .AddOtlpExporter())
                .WithMetrics(metrics => metrics
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation());

            builder.Services.AddSingleton<SlowQueryInterceptor>();

            builder.Services.AddDbContext<AppDbContext>((sp, options) =>
                options.UseNpgsql(
                    builder.Configuration.GetConnectionString("AppDb"),
                    na => na.MigrationsAssembly("Infrastructure"))
                .AddInterceptors(sp.GetRequiredService<SlowQueryInterceptor>()));

            builder.Services.AddControllers();
            builder.Services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            builder.Services.AddProblemDetails();

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
            builder.Services.AddTransient<IStatisticsRepository, StatisticsRepository>();
            builder.Services.AddTransient<ISavingsPotRepository, SavingsPotRepository>();
            builder.Services.AddTransient<ISavingsPotInviteRepository, SavingsPotInviteRepository>();

            builder.Services.AddTransient<IRegisterUserUseCase, RegisterUserUseCase>();
            builder.Services.AddTransient<IGetUserByIdUseCase, GetUserByIdUseCase>();
            builder.Services.AddTransient<ICreateBudgetUseCase, CreateBudgetUseCase>();
            builder.Services.AddTransient<IGetAllBudgetsUseCase, GetAllBudgetsUseCase>();
            builder.Services.AddTransient<IDeleteBudgetUseCase, DeleteBudgetUseCase>();
            builder.Services.AddTransient<IFinishBudgetPeriodUseCase, FinishBudgetPeriodUseCase>();
            builder.Services.AddTransient<ICreateBudgetCategoryUseCase, CreateBudgetCategoryUseCase>();
            builder.Services.AddTransient<IDeleteBudgetCategoryUseCase, DeleteBudgetCategoryUseCase>();
            builder.Services.AddTransient<IUpdateBudgetCategoryNameUseCase, UpdateBudgetCategoryNameUseCase>();
            builder.Services.AddTransient<ICreateSpendingUseCase, CreateSpendingUseCase>();
            builder.Services.AddTransient<IDeleteSpendingUseCase, DeleteSpendingUseCase>();
            builder.Services.AddTransient<ICreateBudgetInviteUseCase, CreateBudgetInviteUseCase>();
            builder.Services.AddTransient<IUpdateBudgetInviteResponseUseCase, UpdateBudgetInviteResponseUseCase>();
            builder.Services.AddTransient<IGetAllCurrenciesUseCase, GetAllCurrenciesUseCase>();
            builder.Services.AddTransient<IGetPeriodComparisonUseCase, GetPeriodComparisonUseCase>();
            builder.Services.AddTransient<IGetAllSavingsPotsUseCase, GetAllSavingsPotsUseCase>();
            builder.Services.AddTransient<ICreateSavingsPotUseCase, CreateSavingsPotUseCase>();
            builder.Services.AddTransient<IDeleteSavingsPotUseCase, DeleteSavingsPotUseCase>();
            builder.Services.AddTransient<IAddSavingsContributionUseCase, AddSavingsContributionUseCase>();
            builder.Services.AddTransient<IRemoveSavingsContributionUseCase, RemoveSavingsContributionUseCase>();
            builder.Services.AddTransient<IGetSavingsHistoryUseCase, GetSavingsHistoryUseCase>();
            builder.Services.AddTransient<ISendSavingsPotInviteUseCase, SendSavingsPotInviteUseCase>();
            builder.Services.AddTransient<IUpdateSavingsPotInviteResponseUseCase, UpdateSavingsPotInviteResponseUseCase>();

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

            string? saltEdgeAppId = builder.Configuration.GetSection("SaltEdge:AppId").Value;
            string? saltEdgeSecret = builder.Configuration.GetSection("SaltEdge:Secret").Value;
            string? saltEdgeBaseUrl = builder.Configuration.GetSection("SaltEdge:BaseUrl").Value;

            if (string.IsNullOrWhiteSpace(saltEdgeAppId))
            {
                throw new InvalidOperationException("SaltEdge:AppId is not configured in appsettings.json or environment variables.");
            }

            if (string.IsNullOrWhiteSpace(saltEdgeSecret))
            {
                throw new InvalidOperationException("SaltEdge:Secret is not configured in appsettings.json or environment variables.");
            }

            if (string.IsNullOrWhiteSpace(saltEdgeBaseUrl))
            {
                throw new InvalidOperationException("SaltEdge:BaseUrl is not configured in appsettings.json or environment variables.");
            }

            builder.Services.AddTransient<IPushNotificationService, PushNotificationsService>();

            builder.Services.AddEnableBankingApi(options =>
            {
                options.KeyPath = Path.Combine(builder.Environment.ContentRootPath, bankingCertificatePath);
                options.AppKid = bankingAppKeyId;
            });

            builder.Services.AddSaltEdgeApi(options =>
            {
                options.AppId = saltEdgeAppId;
                options.Secret = saltEdgeSecret;
                options.BaseUrl = saltEdgeBaseUrl;
            });

            builder.Services.AddTransient<IGetBanksUseCase, GetBanksUseCase>();
            builder.Services.AddTransient<IStartBankConnectionUseCase, StartBankConnectionUseCase>();
            builder.Services.AddTransient<IFinishBankConnectionUseCase, FinishBankConnectionUseCase>();
            builder.Services.AddTransient<ICategorizeTransactionsUseCase, CategorizeTransactionsUseCase>();
            builder.Services.AddTransient<IGetUncategorizedTransactionsUseCase, GetUncategorizedTransactionsUseCase>();
            builder.Services.AddTransient<IRemoveConnectedBankBySessionIdUseCase, RemoveConnectedBankBySessionIdUseCase>();
            builder.Services.AddTransient<IBankSyncWorker, BankSyncWorker>();

            builder.Services.AddTransient<IGetSaltEdgeBanksUseCase, GetSaltEdgeBanksUseCase>();
            builder.Services.AddTransient<IGetConnectedSaltEdgeBanksByUserUseCase, GetConnectedSaltEdgeBanksByUserUseCase>();
            builder.Services.AddTransient<IGetSaltEdgeConnectionStatusUseCase, GetSaltEdgeConnectionStatusUseCase>();
            builder.Services.AddTransient<IStartSaltEdgeConnectionUseCase, StartSaltEdgeConnectionUseCase>();
            builder.Services.AddTransient<IFinishSaltEdgeConnectionUseCase, FinishSaltEdgeConnectionUseCase>();
            builder.Services.AddTransient<IFailSaltEdgeConnectionUseCase, FailSaltEdgeConnectionUseCase>();
            builder.Services.AddTransient<IRemoveSaltEdgeConnectionUseCase, RemoveSaltEdgeConnectionUseCase>();
            builder.Services.AddTransient<ISaltEdgeSyncWorker, SaltEdgeSyncWorker>();

            builder.Services.AddTransient<IBankConsentRepository, BankConsentRepository>();
            builder.Services.AddTransient<IBankAccountRepository, BankAccountRepository>();
            builder.Services.AddTransient<IBankTransactionRepository, BankTransactionRepository>();
            builder.Services.AddTransient<ISaltEdgeCustomerRepository, SaltEdgeCustomerRepository>();
            builder.Services.AddTransient<ISaltEdgeConnectionRepository, SaltEdgeConnectionRepository>();
            builder.Services.AddTransient<ISaltEdgeAccountRepository, SaltEdgeAccountRepository>();
            builder.Services.AddTransient<ISaltEdgeTransactionRepository, SaltEdgeTransactionRepository>();

            //builder.Services.AddHostedService<TransactionSyncBackgroundService>();
            //builder.Services.AddHostedService<SaltEdgeTransactionSyncBackgroundService>();

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

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseExceptionHandler();
            app.UseMiddleware<CorrelationIdMiddleware>();
            app.UseSerilogRequestLogging(opts =>
            {
                opts.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
                opts.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
                {
                    diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                    var userId = httpContext.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                    if (userId is not null)
                        diagnosticContext.Set("UserId", userId);
                };
                opts.GetLevel = (httpContext, elapsed, ex) =>
                    ex is not null ? LogEventLevel.Error :
                    elapsed > 1000 ? LogEventLevel.Warning :
                    LogEventLevel.Information;
            });
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseMiddleware<UserIdEnricherMiddleware>();

            app.MapSwagger();
            app.MapControllers();

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<AppDbContext>();
                    Log.Information("Applying database migrations...");
                    context.Database.Migrate();
                    Log.Information("Database migrations applied successfully.");
                }
                catch (Exception ex)
                {
                    Log.Fatal(ex, "An error occurred during database migration.");
                }
            }

            app.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application startup failed.");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
