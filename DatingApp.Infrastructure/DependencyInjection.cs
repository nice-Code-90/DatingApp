using DatingApp.Application.Interfaces;
using DatingApp.Infrastructure.Data;
using DatingApp.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DatingApp.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPhotoService, PhotoService>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IAiHelperService, AiHelperService>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IGeocodingService, GeocodingService>();
        services.AddScoped<ICacheService, InMemoryCacheService>();
        services.AddScoped<IDbInitializer, DbInitializer>();

        

        return services;
    }
}
