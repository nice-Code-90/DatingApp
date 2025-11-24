using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using DatingApp.Application.Extensions;
using Microsoft.AspNetCore.SignalR;

namespace DatingApp.Presentation.SignalR;


[Authorize]
public class PresenceHub(PresenceTracker presenceTracker) : Hub
{
    public override async Task OnConnectedAsync()
    {
        await presenceTracker.UserConnected(GetUserId(), Context.ConnectionId);

        await Clients.Others.SendAsync("UserOnline", GetUserId());

        var currentUsers = await presenceTracker.GetOnlineUsers();
        await Clients.Caller.SendAsync("GetnlineUsers", currentUsers);

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await presenceTracker.UserDisconnected(GetUserId(), Context.ConnectionId);

        await Clients.Others.SendAsync("UserOffline", GetUserId());

        var currentUsers = await presenceTracker.GetOnlineUsers();

        await Clients.Caller.SendAsync("GetnlineUsers", currentUsers);

        await base.OnDisconnectedAsync(exception);
    }
    private string GetUserId()
    {
        return Context.User?.GetMemberId() ?? throw new HubException("Cannot get member Id");
    }
}
