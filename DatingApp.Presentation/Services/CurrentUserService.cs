using System.Security.Claims;
using DatingApp.Application.Extensions;
using DatingApp.Application.Interfaces;

namespace DatingApp.Presentation.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public string? MemberId => httpContextAccessor.HttpContext?.User?.GetMemberId();
}
