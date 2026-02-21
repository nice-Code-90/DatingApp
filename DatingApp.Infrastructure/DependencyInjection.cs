using DatingApp.Application.Interfaces;
using DatingApp.Application.Services;
using DatingApp.Infrastructure.Data;
using DatingApp.Infrastructure.Configuration;
using DatingApp.Infrastructure.Repository;
using DatingApp.Infrastructure.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenAI;
using Qdrant.Client;
using System.ClientModel;

namespace DatingApp.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        services.Configure<CerebrasSettings>(configuration.GetSection("CerebrasSettings"));
        services.Configure<QdrantSettings>(configuration.GetSection("Qdrant"));


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
        services.AddScoped<IDatingAgentTools, DatingAgentTools>();
        services.AddScoped<IDatingAgentService, DatingAgentService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        services.AddSingleton<IChatClient>(sp =>
        {
            var cerebrasSettings = sp.GetRequiredService<IOptions<CerebrasSettings>>().Value;
            var apiKey = cerebrasSettings.ApiKey ?? throw new InvalidOperationException("Cerebras API Key is missing from configuration.");
            return new OpenAIClient(new ApiKeyCredential(apiKey), new OpenAIClientOptions { Endpoint = new Uri("https://api.cerebras.ai/v1") })
                .GetChatClient("gpt-oss-120b")
                .AsIChatClient();
        });

        services.AddSingleton(sp =>
        {
            var qdrantSettings = sp.GetRequiredService<IOptions<QdrantSettings>>().Value;
            var qdrantUrl = qdrantSettings.Url ?? throw new InvalidOperationException("Qdrant Url is missing from the configuration");
            var apiKey = qdrantSettings.ApiKey;

            return new QdrantClient(address: new Uri(qdrantUrl), apiKey: apiKey);
        });


        services.AddHttpClient<IEmbeddingGenerator<string, Embedding<float>>, HuggingFaceEmbeddingGenerator>();

        services.AddScoped<IAiMatchmakingService, AiMatchmakingService>();
        return services;
    }
}