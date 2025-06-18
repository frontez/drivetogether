using Microsoft.EntityFrameworkCore;
using EFCore.NamingConventions;
using TripService.Data;
using Prometheus;
using TripService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddMetrics();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<AppDbContext>(options => options
    .UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
    .UseSnakeCaseNamingConvention());
builder.Services.AddScoped<ITripService, TripService.TripService>();
builder.Services.AddSingleton<MessagePublisher>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
//app.UseAuthorization();

app.MapControllers();

app.MapGet("/health/", () => Results.Json(new { status = "OK" }));

app.UseRouting();
app.UseHttpMetrics(); // Add HTTP metrics
app.MapMetrics(); // Map the Prometheus endpoint at /metrics
app.MapControllers();

app.Run();