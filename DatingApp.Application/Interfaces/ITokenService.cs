using DatingApp.Domain.Entities;

namespace DatingApp.Application.Interfaces;

public interface ITokenService
{
    Task<string> CreateToken(AppUser user);
    string GenerateRefreshToken();
}
