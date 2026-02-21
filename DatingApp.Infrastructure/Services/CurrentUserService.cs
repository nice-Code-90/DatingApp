using DatingApp.Application.Extensions;
using DatingApp.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace DatingApp.Infrastructure.Services;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public string? MemberId => httpContextAccessor.HttpContext?.User?.GetMemberId();
}
