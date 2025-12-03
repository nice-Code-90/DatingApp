using DatingApp.Application.Interfaces;
using DatingApp.Infrastructure.Data;
using DatingApp.Infrastructure.Repository;
using DatingApp.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.Connectors.Google;
using Microsoft.Extensions.AI;

namespace DatingApp.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPhotoService, PhotoService>();
        services.AddScoped<IMemberRepository, MemberRepository>();
        services.AddScoped<IMessageRepository, MessageRepository>();
        services.AddScoped<ILikesRepository, LikesRepository>();
        services.AddScoped<IPhotoRepository, PhotoRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IAiHelperService, AiHelperService>();
        services.AddScoped<IGeocodingService, GeocodingService>();
        services.AddScoped<ICacheService, InMemoryCacheService>();
        services.AddScoped<IDbInitializer, DbInitializer>();
        services.AddScoped<IDataSeedingService, DataSeedingService>();

        services.AddKernel();

        services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(sp =>
        {
            var apiKey = configuration["GeminiSettings:ApiKey"];
            if (string.IsNullOrEmpty(apiKey)) throw new Exception("Gemini API Key is missing");
            return new GoogleAIEmbeddingGenerator(
                modelId: "text-embedding-004",
                apiKey: apiKey,
                apiVersion: GoogleAIVersion.V1_Beta
            );
        });

        services.AddScoped<IAiMatchmakingService, AiMatchmakingService>();
        return services;
    }
}