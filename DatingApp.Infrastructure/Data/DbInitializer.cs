using DatingApp.Application.Interfaces;
using DatingApp.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DatingApp.Infrastructure.Data;

public class DbInitializer(
    ILogger<DbInitializer> logger,
    AppDbContext context,
    UserManager<AppUser> userManager,
    IGeocodingService geocodingService,
    IAiMatchmakingService aiMatchmakingService) : IDbInitializer
{
    public async Task InitializeAsync()
    {
        try
        {
            await context.Database.MigrateAsync();
            await context.Connections.ExecuteDeleteAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred during database migration.");
            // We don't re-throw here to allow the app to start even if DB migration fails.
        }

        try
        {
            await Seed.SeedAdmin(userManager);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred during admin seeding. The app will continue to start.");
        }
    }
}