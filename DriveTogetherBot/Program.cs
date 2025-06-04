using DriveTogetherBot;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

IHostApplicationLifetime lifetime = app.Lifetime;
AppDomain.CurrentDomain.ProcessExit += new EventHandler((s,e) => lifetime.StopApplication());

TelegramHandler.Initialize(builder.Configuration);

if (lifetime != null)
{
    var token = lifetime.ApplicationStopping;
    var telegramHandler = TelegramHandler.GetInstance(token);
    telegramHandler.Start();
}

app.MapGet("/health/", () => Results.Json(new { status = "OK" }));

app.Run();