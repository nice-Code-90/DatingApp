using System;
using DatingApp.Application.Extensions;
using DatingApp.Application.Interfaces;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DatingApp.Presentation.Helpers;

public class LogUserActivity : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var resultContext = await next();

        if (resultContext.HttpContext.User.Identity?.IsAuthenticated != true)
            return;

        var memberId = resultContext.HttpContext.User.GetMemberId();

        var uow = resultContext.HttpContext.RequestServices.GetRequiredService<IUnitOfWork>();

        var member = await uow.MemberRepository.GetMemberByIdAsync(memberId);
        if (member == null) return;
        member.LastActive = DateTime.UtcNow;
        await uow.Complete();
    }
}
