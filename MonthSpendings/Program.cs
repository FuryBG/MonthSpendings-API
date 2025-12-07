using Application.Interfaces;
using Application.Interfaces.Repository;
using Application.Services;
using Application.UseCases;
using Infrastructure;
using Infrastructure.Repository;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
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
        builder.Services.AddOpenApi();
        builder.Services.AddHttpContextAccessor();

        builder.Services.AddTransient<ITokenService, TokenService>();

        builder.Services.AddScoped<IUserService, UserService>();

        builder.Services.AddTransient<IUnitOfWork, UnitOfWork>();
        builder.Services.AddTransient<IUserRepository, UserRepository>();
        builder.Services.AddTransient<IBudgetRepository, BudgetRepository>();
        builder.Services.AddTransient<IBudgetCategoryRepository, BudgetCategoryRepository>();
        builder.Services.AddTransient<ICategorySpendingsRepository, CategorySpendingsRepository>();
        builder.Services.AddTransient<IBudgetInviteRepository, BudgetInviteRepository>();

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


        builder.Services.AddTransient<IPushNotificationsService, PushNotificationsService>();

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
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        //app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapSwagger();
        app.MapControllers();

        app.Run();
    }
}
