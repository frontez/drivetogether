using DriveTogetherBot;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Add Redis caching
builder.Services.AddStackExchangeRedisCache(options =>
{
    var redisConfig = builder.Configuration.GetConnectionString("Redis");
    Console.WriteLine($"Redis Configuration: {redisConfig}");

    if (string.IsNullOrEmpty(redisConfig))
    {
        throw new Exception("Redis connection string is not configured");
    }

    options.Configuration = redisConfig;
    options.InstanceName = "DriveTogetherBot_";
});

builder.Services.AddSingleton<RedisCacheService>();

var app = builder.Build();

app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Request started: {Method} {Path}", 
        context.Request.Method, context.Request.Path);
    
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Request failed");
        throw;
    }
    
    logger.LogInformation("Request completed: {StatusCode}", context.Response.StatusCode);
});

IHostApplicationLifetime lifetime = app.Lifetime;
AppDomain.CurrentDomain.ProcessExit += new EventHandler((s, e) => lifetime.StopApplication());

var redisService = app.Services.GetRequiredService<RedisCacheService>();
var logger = app.Services.GetRequiredService<ILogger<RedisCacheService>>();

TelegramHandler.Initialize(builder.Configuration, redisService, logger);

if (lifetime != null)
{
    var token = lifetime.ApplicationStopping;
    var telegramHandler = TelegramHandler.GetInstance(token);
    telegramHandler.Start();
}

app.MapGet("/health/", () => Results.Json(new { status = "OK" }));

app.Run();