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
using Microsoft.OpenApi.Models;




var builder = WebApplication.CreateBuilder(args);



builder.Services.AddControllers().AddJsonOptions(options =>
{
    
    options.JsonSerializerOptions.Converters.Add(new NetTopologySuite.IO.Converters.GeoJsonConverterFactory());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme.",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddDbContext<AppDbContext>(opt =>
{
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        
        sqlServerOptions => sqlServerOptions.UseNetTopologySuite()
    );
});
builder.Services.AddCors();
builder.Services.AddHttpClient(); 

builder.Services.Configure<CloudinarySettings>(builder.Configuration.GetSection("CloudinarySettings"));
builder.Services.Configure<OpenCageSettings>(builder.Configuration.GetSection("OpenCageSettings")); 
builder.Services.Configure<GeminiSettings>(builder.Configuration.GetSection("GeminiSettings")); 

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
builder.Services.AddScoped<IMemberService, MemberService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<ILikesService, LikesService>();
builder.Services.AddScoped<IGeocodingService, GeocodingService>();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<ICacheService, InMemoryCacheService>();

builder.Services.AddScoped<LogUserActivity>();


var app = builder.Build();


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
}

app.UseMiddleware<ExceptionMiddleware>();


app.Use(async (context, next) =>
{
    
    context.Response.Headers.Append("X-Frame-Options", "SAMEORIGIN");
    
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    
    context.Response.Headers.Append("Permissions-Policy", "camera=(), microphone=()");

    var csp = new StringBuilder()
        .Append("default-src 'self'; ")
        .Append("script-src 'self' 'unsafe-inline'; ")
        .Append("style-src 'self' 'unsafe-inline' https://fonts.googleapis.com; ")
        .Append("img-src 'self' data: https://res.cloudinary.com https://randomuser.me; ")
        .Append("font-src 'self'; ")
        .Append("connect-src 'self' https://api.opencagedata.com");

    if (app.Environment.IsDevelopment())
    {
        csp.Append(" ws://localhost:* wss://localhost:*;");
    }
    csp.Append("; ")
        .Append("frame-src 'self'; ")
        .Append("object-src 'none'; ")
        .Append("base-uri 'self'; ")
        .Append("form-action 'self';");
    context.Response.Headers.Append("Content-Security-Policy", csp.ToString());

    await next();
});

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
    var geocodingService = services.GetRequiredService<IGeocodingService>();
    await context.Database.MigrateAsync();
    await context.Connections.ExecuteDeleteAsync();
    await Seed.SeedUsers(userManager, geocodingService);
}
catch (Exception ex)
{
    var logger = services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred during migration");
}

app.Run();