using System.Security.Cryptography;
using System.Text;
using System.Reflection;
using System.Text.Json;
using DatingApp.Application.DTOs;
using DatingApp.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.Infrastructure.Data;

public class Seed
{
    public static async Task SeedUsers(UserManager<AppUser> userManager)
    {
        if (await userManager.Users.AnyAsync()) return;

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

            user.Member.Photos.Add(new Photo
            {
                Url = member.ImageUrl!,
                MemberId = member.Id,
                IsApproved = true
            });

            var result = await userManager.CreateAsync(user, "Pa$$w0rd");

            if (!result.Succeeded)
            {
                Console.WriteLine(result.Errors.First().Description);
            }
            await userManager.AddToRoleAsync(user, "Member");
        }

        var admin = new AppUser
        {
            UserName = "admin@test.com",
            Email = "admin@test.com",
            DisplayName = "Admin"
        };

        await userManager.CreateAsync(admin, "Pa$$w0rd");
        await userManager.AddToRolesAsync(admin, ["Admin", "Moderator"]);
    }
}