using APPLICATION.Services;
using CORE.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace APPLICATION;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Register application services
        services.AddScoped<IAuthService, AuthService>();
        
        // Note: Infrastructure services (IUserRepository, ITokenService, IPasswordHasher) 
        // should be registered in the INFRASTRUCTURE project's DependencyInjection
        
        return services;
    }
}

