using DatingApp.Application.DTOs;
using DatingApp.Domain.Entities;
using DatingApp.Application.Extensions;
using DatingApp.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.Presentation.Controllers;

public class AccountController(UserManager<AppUser> userManager, ITokenService tokenService, IGeocodingService geocodingService, ICacheService cacheService, IAiMatchmakingService aiMatchmakingService
) : BaseApiController
{
    [HttpPost("register")]
    public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
    {
        if (await userManager.Users.AnyAsync(x => x.Email == registerDto.Email)) return BadRequest("Email is taken");

        var user = new AppUser
        {
            DisplayName = registerDto.DisplayName,
            Email = registerDto.Email,
            UserName = registerDto.Email,
            Member = new Member
            {
                DisplayName = registerDto.DisplayName,
                Gender = registerDto.Gender,
                City = registerDto.City,
                Country = registerDto.Country,
                DateOfBirth = registerDto.DateOfBirth
            }
        };

        user.Member.Location = await geocodingService.GetCoordinatesForAddressAsync(registerDto.City, registerDto.Country);

        var result = await userManager.CreateAsync(user, registerDto.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("identity", error.Description);
            }

            return ValidationProblem();
        }
        await userManager.AddToRoleAsync(user, "Member");

        await aiMatchmakingService.UpdateMemberProfileAsync(user.Member);

        await cacheService.RemoveByPrefixAsync("members:");

        return await CreateUserDtoWithCookie(user);
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
    {
        var user = await userManager.FindByEmailAsync(loginDto.Email);

        if (user == null) return Unauthorized("Invalid email address");

        var result = await userManager.CheckPasswordAsync(user, loginDto.Password);


        if (!result) return Unauthorized("Invalid password");

        return await CreateUserDtoWithCookie(user);
    }

    [HttpPost("refresh-token")]
    public async Task<ActionResult<UserDto>> RefreshToken()
    {
        var refreshToken = Request.Cookies["refreshToken"];

        if (refreshToken == null) return NoContent();

        var user = await userManager.Users.
            FirstOrDefaultAsync(x => x.RefreshToken == refreshToken
                && x.RefreshTokenExpiry > DateTime.UtcNow);


        if (user == null) return Unauthorized();

        return await CreateUserDtoWithCookie(user);
    }

    private async Task<ActionResult<UserDto>> CreateUserDtoWithCookie(AppUser user)
    {
        await SetRefreshTokenCookie(user);
        return await user.ToDto(tokenService);
    }

    private async Task SetRefreshTokenCookie(AppUser user)
    {
        var refreshToken = tokenService.GenerateRefreshToken();
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        await userManager.UpdateAsync(user);

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTime.UtcNow.AddDays(7),
            Path = "/"
        };

        Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<ActionResult> Logout()
    {
        await userManager.Users
            .Where(x => x.Id == User.GetMemberId())
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.RefreshToken, _ => null)
                .SetProperty(x => x.RefreshTokenExpiry, _ => null)
                );

        Response.Cookies.Delete("refreshToken");

        return Ok();
    }
}
