using DatingApp.Application.Interfaces;
using DatingApp.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DatingApp.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IMemberService, MemberService>();
        services.AddScoped<IMessageService, MessageService>();
        services.AddScoped<IAdminService, AdminService>();
        services.AddScoped<ILikesService, LikesService>();

        return services;
    }
}
