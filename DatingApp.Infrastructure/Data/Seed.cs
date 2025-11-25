using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DatingApp.Application.DTOs;
using DatingApp.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using NetTopologySuite.Geometries;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.Infrastructure.Data;

public static class Seed
{
    public static async Task SeedUsers(UserManager<AppUser> userManager)
    {
        if (await userManager.Users.AnyAsync()) return;

        var memberData = await File.ReadAllTextAsync("Data/UserSeedData.json");
        var members = JsonSerializer.Deserialize<List<SeedUserDto>>(memberData);

        if (members == null)
        {
            Console.WriteLine("No members in seed data");
            return;
        }

        // Létrehozunk egy GeometryFactory-t a Point objektumok gyártásához
        var geometryFactory = new GeometryFactory(new PrecisionModel(), 4326);

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
                    Created = member.Created,
                    Location = geometryFactory.CreatePoint(new Coordinate(0, 0)) 
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
