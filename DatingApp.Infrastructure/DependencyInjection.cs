using DatingApp.Application.Interfaces;
using DatingApp.Infrastructure.AI;
using DatingApp.Infrastructure.Data;
using DatingApp.Infrastructure.Repository;
using DatingApp.Infrastructure.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;

namespace DatingApp.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        var cerebrasApiKey = configuration["CerebrasSettings:ApiKey"]
        ?? throw new InvalidOperationException("Cerebras API Key is missing from configuration.");

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

        services.AddSingleton<IChatClient>(sp =>
            new OpenAIClient(
                new ApiKeyCredential(cerebrasApiKey),
                new OpenAIClientOptions { Endpoint = new Uri("https://api.cerebras.ai/v1") }
            ).GetChatClient("gpt-oss-120b").AsIChatClient());

        services.AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(sp =>
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var modelPath = Path.Combine(baseDir, "Data", "model.onnx");
            var vocabPath = Path.Combine(baseDir, "Data", "vocab.txt");
            var logger = sp.GetRequiredService<ILogger<OnnxLocalEmbeddingGenerator>>();
            return new OnnxLocalEmbeddingGenerator(modelPath, vocabPath, logger);
        });

        services.AddScoped<IAiMatchmakingService, AiMatchmakingService>();
        return services;
    }
}