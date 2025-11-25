using System.Text;
using DatingApp.Infrastructure.Data;
using DatingApp.Domain.Entities;
using DatingApp.Application.Interfaces;
using DatingApp.Application.Helpers;
using DatingApp.Presentation.Middleware;
using DatingApp.Infrastructure.Services;
using DatingApp.Presentation.SignalR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using DatingApp.Presentation.Helpers;


var builder = WebApplication.CreateBuilder(args);



builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(opt =>
{
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        
        sqlServerOptions => sqlServerOptions.UseNetTopologySuite()
    );
});
builder.Services.AddCors();
builder.Services.AddHttpClient(); 

builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));

builder.Services.AddSignalR();
builder.Services.AddSingleton<PresenceTracker>();

builder.Services.AddIdentityCore<AppUser>(opt =>
{
    opt.Password.RequireNonAlphanumeric = false;
    opt.User.RequireUniqueEmail = true;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var tokenKey = builder.Configuration["TokenKey"]
             ?? throw new Exception("Token key not found - Program.cs");
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey)),
            ValidateIssuer = false,
            ValidateAudience = false
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accesToken = context.Request.Query["access_token"];

                var path = context.HttpContext.Request.Path;

                if (!string.IsNullOrEmpty(accesToken) && path.StartsWithSegments("/hubs"))
                {
                    context.Token = accesToken;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"))
    .AddPolicy("ModeratePhotoRole", policy => policy.RequireRole("Admin", "Moderator"));

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IPhotoService, PhotoService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IAiHelperService, AiHelperService>();
builder.Services.AddScoped<IGeocodingService, GeocodingService>(); 

builder.Services.AddScoped<LogUserActivity>();


var app = builder.Build();


app.UseMiddleware<ExceptionMiddleware>();
app.UseCors(x => x
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()
    .WithOrigins("http://localhost:4200", "https://localhost:4200"));

app.UseAuthentication();
app.UseAuthorization();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();
app.MapHub<PresenceHub>("hubs/presence");
app.MapHub<MessageHub>("hubs/messages");
app.MapFallbackToController("Index", "Fallback");

using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
try
{
    var context = services.GetRequiredService<AppDbContext>();
    var userManager = services.GetRequiredService<UserManager<AppUser>>();
    await context.Database.MigrateAsync();
    await context.Connections.ExecuteDeleteAsync();
    await Seed.SeedUsers(userManager);
}
catch (Exception ex)
{
    var logger = services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred during migration");
}

app.Run();