using Microsoft.EntityFrameworkCore;
using NotificationService;
using NotificationService.Data;
using NotificationService.Services;

var builder = Host.CreateApplicationBuilder(args);

// Add DbContext
builder.Services.AddDbContext<NotificationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services
builder.Services.AddScoped<EmailService>();

// Add worker
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
