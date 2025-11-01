using Application.Interfaces;
using Application.Interfaces.Repository;
using Infrastructure;
using Infrastructure.Repository;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

namespace MonthSpendings;
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("AppDb"), na => na.MigrationsAssembly("Infrastructure")));

        builder.Services.AddControllers();
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        builder.Services.AddTransient<ITokenService, TokenService>();


        builder.Services.AddTransient<IUnitOfWork, UnitOfWork>();
        builder.Services.AddTransient<IUserRepository, UserRepository>();
        builder.Services.AddTransient<IBudgetRepository, BudgetRepository>();
        builder.Services.AddTransient<IBudgetCategoryRepository, BudgetCategoryRepository>();
        builder.Services.AddTransient<ICategorySpendingsRepository, CategorySpendingsRepository>();

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(jwtOptions =>
        {
            jwtOptions.Authority = builder.Configuration["Jwt:Issuer"];
            jwtOptions.Audience = builder.Configuration["Jwt:Audience"];
        });


        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapSwagger();
        app.MapControllers();

        app.Run();
    }
}
