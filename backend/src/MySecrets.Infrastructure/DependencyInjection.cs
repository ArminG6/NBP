using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using MySecrets.Application.Common.Interfaces;
using MySecrets.Domain.Interfaces;
using MySecrets.Infrastructure.MongoDB;
using MySecrets.Infrastructure.Persistence;
using MySecrets.Infrastructure.Services;

namespace MySecrets.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // SQL Server
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        // MongoDB
        services.Configure<MongoDbSettings>(
            configuration.GetSection("MongoDB"));
        services.AddSingleton<MongoDbContext>();

        // Repositories
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Services
        services.Configure<EncryptionSettings>(options =>
        {
            options.MasterKey = configuration["ENCRYPTION_MASTER_KEY"] ?? 
                               configuration["EncryptionSettings:MasterKey"] ?? "";
        });
        services.AddSingleton<IEncryptionService, EncryptionService>();

        services.Configure<JwtSettings>(options =>
        {
            options.SecretKey = configuration["JWT_SECRET_KEY"] ?? 
                               configuration["JwtSettings:SecretKey"] ?? "";
            options.Issuer = configuration["JwtSettings:Issuer"] ?? "MySecrets";
            options.Audience = configuration["JwtSettings:Audience"] ?? "MySecretsApp";
            options.AccessTokenExpirationMinutes = 
                int.TryParse(configuration["JwtSettings:AccessTokenExpirationMinutes"], out var mins) 
                    ? mins : 15;
            options.RefreshTokenExpirationDays = 
                int.TryParse(configuration["JwtSettings:RefreshTokenExpirationDays"], out var days) 
                    ? days : 7;
        });
        services.AddSingleton<IJwtService, JwtService>();

        services.Configure<GoogleAuthSettings>(
            configuration.GetSection("GoogleAuth"));
        services.AddScoped<IGoogleAuthService, GoogleAuthService>();

        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // HTTP Context Accessor
        services.AddHttpContextAccessor();

        // JWT Authentication
        var jwtSecretKey = configuration["JWT_SECRET_KEY"] ?? 
                          configuration["JwtSettings:SecretKey"] ?? "";
        
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = configuration["JwtSettings:Issuer"] ?? "MySecrets",
                ValidAudience = configuration["JwtSettings:Audience"] ?? "MySecretsApp",
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
                ClockSkew = TimeSpan.Zero
            };
        });

        return services;
    }
}
