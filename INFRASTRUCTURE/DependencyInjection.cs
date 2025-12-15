using INFRASTRUCTURE.Persistence;
using INFRASTRUCTURE.Repositories;
using INFRASTRUCTURE.Services;
using CORE.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace INFRASTRUCTURE;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrEmpty(connectionString))
        {
            throw new ArgumentNullException(nameof(connectionString), "Database connection string is missing from configuration.");
        }

        // Register DbContext
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Register repositories
        services.AddScoped<IUserRepository, UserRepository>();

        // Register services
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();

        return services;
    }
}

