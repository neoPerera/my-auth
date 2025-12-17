using INFRASTRUCTURE.Persistence;
using INFRASTRUCTURE.Providers;
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
        // Get authentication provider from configuration
        var authProvider = configuration["Authentication:Provider"] ?? "PostgreSQL";

        // Register authentication provider based on configuration
        switch (authProvider.ToUpperInvariant())
        {
            case "POSTGRESQL":
                var connectionString = configuration.GetConnectionString("DefaultConnection");

                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new ArgumentNullException(nameof(connectionString), "Database connection string is missing from configuration.");
                }

                // Register DbContext
                services.AddDbContext<AppDbContext>(options =>
                    options.UseNpgsql(connectionString));

                // Register PostgreSQL authentication provider
                services.AddScoped<IAuthenticationProvider, PostgreSqlAuthenticationProvider>();
                break;

            case "MOCK":
                // Register Mock authentication provider
                services.AddSingleton<IAuthenticationProvider, MockAuthenticationProvider>();
                break;

            // Add other providers here in the future
            // case "LDAP":
            //     services.AddScoped<IAuthenticationProvider, LdapAuthenticationProvider>();
            //     break;
            // case "ACTIVEDIRECTORY":
            //     services.AddScoped<IAuthenticationProvider, ActiveDirectoryAuthenticationProvider>();
            //     break;

            default:
                throw new NotSupportedException($"Authentication provider '{authProvider}' is not supported. Supported providers: PostgreSQL, Mock");
        }

        // Get authorization provider from configuration (separate from authentication)
        var authzProvider = configuration["Authorization:Provider"] ?? "Mock";

        // Register authorization provider based on configuration
        switch (authzProvider.ToUpperInvariant())
        {
            case "MOCK":
                // Register Mock authorization provider
                services.AddSingleton<IAuthorizationProvider, MockAuthorizationProvider>();
                break;

            case "POSTGRESQL":
                // Register Database authorization provider
                // Note: Requires AppDbContext to be registered (should be registered with PostgreSQL auth provider)
                services.AddScoped<IAuthorizationProvider, DatabaseAuthorizationProvider>();
                break;

            default:
                throw new NotSupportedException($"Authorization provider '{authzProvider}' is not supported. Supported providers: Mock, PostgreSQL");
        }

        // Register authentication services
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IInputSanitizer, InputSanitizer>();

        // Note: IUserRepository is not registered here as this service only handles authentication.
        // UserRepository can be registered in other services that need user management functionality.

        return services;
    }
}

