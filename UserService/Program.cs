using Microsoft.EntityFrameworkCore;
using EFCore.NamingConventions;
using UserService.Data;
using UserService.Extensions;
using Prometheus;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using UserService.Models;
using System.Text.Json;

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

// Add CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", policy =>
    {
        policy.AllowAnyOrigin() // Allow requests from any origin (you can restrict this to specific origins)
              .AllowAnyMethod() // Allow any HTTP method (GET, POST, etc.)
              .AllowAnyHeader() // Allow any headers, including X-ID-Token
              .WithExposedHeaders("X-ID-Token"); // Expose X-ID-Token in the response if needed
    });
});

var app = builder.Build();

app.UseStaticFiles();

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

app.MapGet("/login", (IConfiguration configuration, HttpContext context, string nonce) =>
{
    var keycloakAuthorizationUrl = configuration["Keycloak:AuthorizationUrl"];
    var clientId = configuration["Keycloak:ClientId"];
    var redirectUri = configuration["Keycloak:RedirectUri"];
    var responseType = "token id_token"; // Request both access_token and id_token
    var scope = "openid profile"; // Scopes to request

    // Include the nonce in the authorization URL
    var authorizationUrl = $"{keycloakAuthorizationUrl}?client_id={clientId}&redirect_uri={redirectUri}&response_type={responseType}&scope={scope}&nonce={nonce}";

    context.Response.Redirect(authorizationUrl);
});

app.MapGet("/logout", (HttpContext context, IConfiguration configuration) =>
{
    var idToken = context.Request.Headers["X-ID-Token"].ToString();
    if (string.IsNullOrEmpty(idToken))
    {
        return Results.BadRequest("No id_token provided.");
    }
    else
    {
        var keycloakLogoutUrl = configuration["Keycloak:LogoutUrl"];
        var clientId = configuration["Keycloak:ClientId"];
        var redirectUri = configuration["Keycloak:RedirectUri"];

        // Construct the logout URL with id_token_hint
        var logoutUrl = $"{keycloakLogoutUrl}?id_token_hint={idToken}&post_logout_redirect_uri={redirectUri}&client_id={clientId}";

        // Return the logout URL to the frontend
        return Results.Ok(new { logoutUrl });
    }
});

app.MapGet("/profile", (IConfiguration configuration) =>
{
    var profileUrl = configuration["Keycloak:ProfileUrl"];
    return Results.Redirect(profileUrl);
}).RequireAuthorization();

// Use the CORS policy
app.UseCors("AllowAllOrigins");

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.UseHttpMetrics(); 
app.MapMetrics(); 

//app.MapControllers();

app.Run();