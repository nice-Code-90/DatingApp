using System.Reflection;
using System.Text;
using System.Text.Json;
using DatingApp.Application.DTOs;
using DatingApp.Application.Interfaces;
using DatingApp.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DatingApp.Infrastructure.Data;

public class Seed
{
    public static async Task SeedAdmin(UserManager<AppUser> userManager)
    {
        if (await userManager.Users.AnyAsync(u => u.UserName == "admin@test.com")) return;

        var admin = new AppUser
        {
            UserName = "admin@test.com",
            Email = "admin@test.com",
            DisplayName = "Admin"
        };

        var adminResult = await userManager.CreateAsync(admin, "Pa$$w0rd");
        if (adminResult.Succeeded)
        {
            await userManager.AddToRolesAsync(admin, ["Admin", "Moderator"]);
        }
    }
    public static async Task SeedUsers(
        ILogger logger,
        UserManager<AppUser> userManager,
        IGeocodingService geocodingService,
        IAiMatchmakingService aiMatchmakingService)
    {
        if (await userManager.Users.AnyAsync(u => u.UserName != "admin@test.com")) return;

        try
        {
            await aiMatchmakingService.InitCollectionAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[AI] Init Failed");
            return;
        }

        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = "DatingApp.Infrastructure.Data.UserSeedData.json";
        string memberData;

        using (var stream = assembly.GetManifestResourceStream(resourceName))
        {
            if (stream == null)
            {
                logger.LogError("Error: Could not find the embedded resource '{resourceName}'.", resourceName);
                return;
            }
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                memberData = await reader.ReadToEndAsync();
            }
        }

        var members = JsonSerializer.Deserialize<List<SeedUserDto>>(memberData);

        if (members == null)
        {
            logger.LogWarning("No members in seed data");
            return;
        }

        foreach (var member in members)
        {
            var user = new AppUser
            {
                Email = member.Email,
                UserName = member.Email,
                DisplayName = member.DisplayName,
                ImageUrl = member.ImageUrl,

                Member = new Member
                {
                    DisplayName = member.DisplayName,
                    Description = member.Description,
                    DateOfBirth = member.DateOfBirth,
                    ImageUrl = member.ImageUrl,
                    Gender = member.Gender,
                    City = member.City,
                    Country = member.Country,
                    LastActive = member.LastActive,
                    Created = member.Created
                }
            };

            var location = await geocodingService.GetCoordinatesForAddressAsync(member.City, member.Country);
            if (location != null && double.IsFinite(location.X) && double.IsFinite(location.Y))
            {
                user.Member.Location = location;
            }

            user.Member.Photos.Add(new Photo
            {
                Url = member.ImageUrl!,
                IsApproved = true
            });

            var result = await userManager.CreateAsync(user, "Pa$$w0rd");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "Member");

                try
                {
                    logger.LogInformation("[AI] Syncing profile for: {DisplayName}...", user.DisplayName);

                    //vector generation by Gemini
                    await aiMatchmakingService.UpdateMemberProfileAsync(user.Member);

                    logger.LogInformation("[AI] -> Success!");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "[AI] Failed to sync {DisplayName}", user.DisplayName);
                }

                logger.LogInformation("[DEBUG] User {DisplayName} processed successfully. Waiting before next...", member.DisplayName);
                await Task.Delay(1100);
            }
            else
            {
                logger.LogError("Error creating user {DisplayName}: {Error}", user.DisplayName, result.Errors.First().Description);
            }
        }
    }
}