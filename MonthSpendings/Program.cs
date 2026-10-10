using Application.Interfaces;
using Application.Interfaces.Repository;
using Application.Localization;
using Application.Options;
using Application.Resources;
using Application.Services;
using Application.UseCases;
using Application.UseCases.NotificationTransactions;
using Application.UseCases.Statistics;
using Infrastructure;
using Infrastructure.Interceptors;
using Infrastructure.Repository;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.WebEncoders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using MonthSpendings.BackgroundServices;
using MonthSpendings.Controllers;
using MonthSpendings.Exceptions;
using MonthSpendings.Filters;
using MonthSpendings.Middleware;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Threading.RateLimiting;

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
                    restrictedToMinimumLevel: LogEventLevel.Information)));

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
            builder.Services.AddRazorPages();
            // Emit Cyrillic/accented text as-is in Razor pages instead of HTML entities.
            builder.Services.Configure<WebEncoderOptions>(o => o.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All));

            builder.Services.Configure<PlanLimitsOptions>(builder.Configuration.GetSection("PlanLimits"));
            builder.Services.Configure<RevenueCatOptions>(builder.Configuration.GetSection("RevenueCat"));
            builder.Services.Configure<AppVersionOptions>(builder.Configuration.GetSection("AppVersion"));

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

            builder.Services.AddRateLimiter(options =>
            {
                options.AddFixedWindowLimiter("auth", o =>
                {
                    o.PermitLimit = 5;
                    o.Window = TimeSpan.FromMinutes(1);
                    o.QueueLimit = 0;
                    o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                });
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                options.OnRejected = async (context, cancellationToken) =>
                {
                    await context.HttpContext.Response.WriteAsJsonAsync(Messages.TooManyRequests, cancellationToken);
                };
            });

            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services.AddTransient<IPasswordService, PasswordService>();
            builder.Services.AddScoped<IUserService, UserService>();

            builder.Services.AddTransient<IUnitOfWork, UnitOfWork>();
            builder.Services.AddTransient<IUserRepository, UserRepository>();
            builder.Services.AddTransient<IBudgetRepository, BudgetRepository>();
            builder.Services.AddTransient<IBudgetCategoryRepository, BudgetCategoryRepository>();
            builder.Services.AddTransient<ICategorySpendingsRepository, CategorySpendingsRepository>();
            builder.Services.AddTransient<IBudgetInviteRepository, BudgetInviteRepository>();
            builder.Services.AddTransient<ICurrencyRepository, CurrencyRepository>();
            builder.Services.AddTransient<IStatisticsRepository, StatisticsRepository>();
            builder.Services.AddTransient<IAccountDeleteRequestRepository, AccountDeleteRequestRepository>();

            builder.Services.AddTransient<IRegisterUserUseCase, RegisterUserUseCase>();
            builder.Services.AddTransient<IRegisterWithEmailUseCase, RegisterWithEmailUseCase>();
            builder.Services.AddTransient<ILoginWithEmailUseCase, LoginWithEmailUseCase>();
            builder.Services.AddTransient<IRefreshTokenUseCase, RefreshTokenUseCase>();
            builder.Services.AddTransient<IRevokeRefreshTokenUseCase, RevokeRefreshTokenUseCase>();
            builder.Services.AddTransient<IGetUserByIdUseCase, GetUserByIdUseCase>();
            builder.Services.AddTransient<IUpdateLastUserActivityUseCase, UpdateLastUserActivityUseCase>();
            builder.Services.AddTransient<IUpdateNotificationTokenUseCase, UpdateNotificationTokenUseCase>();
            builder.Services.AddTransient<IUpdateSyncWalletTransactionsUseCase, UpdateSyncWalletTransactionsUseCase>();
            builder.Services.AddTransient<IUpdateUserLanguageUseCase, UpdateUserLanguageUseCase>();
            builder.Services.AddTransient<IRequestAccountDeletionUseCase, RequestAccountDeletionUseCase>();
            builder.Services.AddTransient<ICreateBudgetUseCase, CreateBudgetUseCase>();
            builder.Services.AddTransient<IGetAllBudgetsUseCase, GetAllBudgetsUseCase>();
            builder.Services.AddTransient<IDeleteBudgetUseCase, DeleteBudgetUseCase>();
            builder.Services.AddTransient<IFinishBudgetPeriodUseCase, FinishBudgetPeriodUseCase>();
            builder.Services.AddTransient<ICreateBudgetCategoryUseCase, CreateBudgetCategoryUseCase>();
            builder.Services.AddTransient<IDeleteBudgetCategoryUseCase, DeleteBudgetCategoryUseCase>();
            builder.Services.AddTransient<IUpdateBudgetCategoryNameUseCase, UpdateBudgetCategoryNameUseCase>();
            builder.Services.AddTransient<ICreateSpendingUseCase, CreateSpendingUseCase>();
            builder.Services.AddTransient<IDeleteSpendingUseCase, DeleteSpendingUseCase>();
            builder.Services.AddTransient<IGetCategorySpendingsByPeriodUseCase, GetCategorySpendingsByPeriodUseCase>();
            builder.Services.AddTransient<ICreateBudgetInviteUseCase, CreateBudgetInviteUseCase>();
            builder.Services.AddTransient<IUpdateBudgetInviteResponseUseCase, UpdateBudgetInviteResponseUseCase>();
            builder.Services.AddTransient<IKickBudgetMemberUseCase, KickBudgetMemberUseCase>();
            builder.Services.AddTransient<ILeaveBudgetUseCase, LeaveBudgetUseCase>();
            builder.Services.AddTransient<IGetAllCurrenciesUseCase, GetAllCurrenciesUseCase>();
            builder.Services.AddTransient<IHandleRevenueCatWebhookUseCase, HandleRevenueCatWebhookUseCase>();
            builder.Services.AddScoped<RevenueCatAuthFilter>();
            builder.Services.AddTransient<IGetRangeSummaryUseCase, GetRangeSummaryUseCase>();

            builder.Services.AddTransient<IPushNotificationService, PushNotificationsService>();

            builder.Services.AddTransient<ICreateNotificationTransactionUseCase, CreateNotificationTransactionUseCase>();
            builder.Services.AddTransient<IGetUncategorizedNotificationTransactionsUseCase, GetUncategorizedNotificationTransactionsUseCase>();
            builder.Services.AddTransient<ICategorizeNotificationTransactionUseCase, CategorizeNotificationTransactionUseCase>();
            builder.Services.AddTransient<IDeleteNotificationTransactionUseCase, DeleteNotificationTransactionUseCase>();

            builder.Services.AddTransient<INotificationTransactionRepository, NotificationTransactionRepository>();
            builder.Services.AddTransient<ITransactionCategoryRuleRepository, TransactionCategoryRuleRepository>();

            builder.Services.AddHostedService<InactivityNotificationBackgroundService>();

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
            app.UseStaticFiles();
            app.UseMiddleware<CorrelationIdMiddleware>();
            app.UseSerilogRequestLogging(opts =>
            {
                opts.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
                opts.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
                {
                    diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
                    var userId = httpContext.User?.FindFirst("sub")?.Value
                        ?? httpContext.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                    if (userId is not null)
                        diagnosticContext.Set("UserId", userId);
                };
                opts.GetLevel = (httpContext, elapsed, ex) =>
                    ex is not null ? LogEventLevel.Error :
                    elapsed > 1000 ? LogEventLevel.Warning :
                    LogEventLevel.Information;
            });
            // Request culture: ?lang= (privacy page links), then Accept-Language. "bg-BG" falls back to "bg";
            // anything unsupported falls back to English. CultureInfo.CurrentUICulture is always en/bg/es afterwards.
            var localizationOptions = new RequestLocalizationOptions()
                .SetDefaultCulture(SupportedLanguages.Default)
                .AddSupportedCultures(SupportedLanguages.All)
                .AddSupportedUICultures(SupportedLanguages.All);
            localizationOptions.RequestCultureProviders =
            [
                new QueryStringRequestCultureProvider { QueryStringKey = "lang", UIQueryStringKey = "lang" },
                new AcceptLanguageHeaderRequestCultureProvider()
            ];
            app.UseRequestLocalization(localizationOptions);
            app.UseRateLimiter();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseMiddleware<UserIdEnricherMiddleware>();

            app.MapSwagger();
            app.MapControllers();
            app.MapRazorPages();

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
