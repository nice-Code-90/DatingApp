using DatingApp.Application.Interfaces;
using DatingApp.Domain.Entities;
using DatingApp.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DatingApp.Infrastructure.Services;

public class DataSeedingService(IServiceProvider serviceProvider) : IDataSeedingService
{
    public void StartSeedUsersProcess()
    {
        _ = Task.Run(async () =>
        {
            using var scope = serviceProvider.CreateScope();

            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            var geocodingService = scope.ServiceProvider.GetRequiredService<IGeocodingService>();
            var aiMatchmakingService = scope.ServiceProvider.GetRequiredService<IAiMatchmakingService>();
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
                .CreateLogger("DatingApp.Infrastructure.Data.Seed");

            await Seed.SeedUsers(logger, userManager, geocodingService, aiMatchmakingService);
        });
    }
}