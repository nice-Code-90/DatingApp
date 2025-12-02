using System.Reflection;
using System.Text;
using System.Text.Json;
using DatingApp.Application.DTOs;
using DatingApp.Application.Interfaces;
using DatingApp.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.Infrastructure.Data;

public class Seed
{
    public static async Task SeedUsers(
        UserManager<AppUser> userManager,
        IGeocodingService geocodingService,
        IAiMatchmakingService aiMatchmakingService)
    {
        if (await userManager.Users.AnyAsync()) return;

        try
        {
            await aiMatchmakingService.InitCollectionAsync();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[AI] Init Failed: {ex.Message}");
            Console.ResetColor();
        }

        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = "DatingApp.Infrastructure.Data.UserSeedData.json";
        string memberData;

        using (var stream = assembly.GetManifestResourceStream(resourceName))
        {
            if (stream == null)
            {
                Console.WriteLine($"Error: Could not find the embedded resource '{resourceName}'.");
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
            Console.WriteLine("No members in seed data");
            return;
        }

        foreach (var member in members)
        {
            var user = new AppUser
            {
                Id = member.Id,
                Email = member.Email,
                UserName = member.Email,
                DisplayName = member.DisplayName,
                ImageUrl = member.ImageUrl,

                Member = new Member
                {
                    Id = member.Id,
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
                MemberId = member.Id,
                IsApproved = true
            });

            var result = await userManager.CreateAsync(user, "Pa$$w0rd");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "Member");

                try
                {
                    Console.WriteLine($"[AI] Syncing profile for: {user.DisplayName}...");

                    //vector generation by Gemini
                    await aiMatchmakingService.UpdateMemberProfileAsync(user.Member);

                    Console.WriteLine($"[AI] -> Success!");
                }
                catch (Exception ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[AI] Failed to sync {user.DisplayName}. Error: {ex.Message}");

                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"[AI] Details: {ex.InnerException.Message}");
                    }
                    Console.ResetColor();
                }
            }
            else
            {
                Console.WriteLine($"Error creating user {user.DisplayName}: {result.Errors.First().Description}");
            }
        }

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
}