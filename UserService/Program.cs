using Microsoft.EntityFrameworkCore;
using EFCore.NamingConventions;
using UserService.Data;
using UserService.Extensions;
using Prometheus;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddMetrics();
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGenWithAuth(builder.Configuration);

builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o=>
    {
        o.RequireHttpsMetadata = false;
        o.Audience = builder.Configuration["Authentication:Audience"];
        o.MetadataAddress = builder.Configuration["Authentication:MetadataAddress"];
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = builder.Configuration["Authentication:ValidIssuer"]
        };
    });

builder.Services.AddDbContext<AppDbContext>(options => options
    .UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
    .UseSnakeCaseNamingConvention());

var app = builder.Build();

if (true)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.MapGet("/health/", () => Results.Json(new { status = "OK" }));
app.MapGet("/users/me", (ClaimsPrincipal claimsPrincipal) =>
{
    return claimsPrincipal.Claims.ToDictionary(c => c.Type, c => c.Value);
}).RequireAuthorization();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.UseHttpMetrics(); 
app.MapMetrics(); 

//app.MapControllers();

app.Run();