using AdmissionService.Data;
using AdmissionService.Jobs;
using AdmissionService.Services;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Quartz;
using System.Text;
using NLog;
using NLog.Web;

// Early init of NLog to allow startup and exception logging, before host is built
var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();
logger.Debug("init main");

try
{

var builder = WebApplication.CreateBuilder(args);

// NLog: Setup NLog for Dependency injection
builder.Logging.ClearProviders();
builder.Host.UseNLog();

// Add services to the container.
builder.Services.AddDbContext<AdmissionDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// Add HttpClient for external API calls
builder.Services.AddHttpClient();

// Register External Dictionary Service
builder.Services.AddScoped<IExternalDictionaryService, ExternalDictionaryService>();

// Add MassTransit with RabbitMQ for event publishing
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"] ?? "rabbitmq", "/", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"] ?? "guest");
            h.Password(builder.Configuration["RabbitMQ:Password"] ?? "guest");
        });
    });
});

// Add Quartz.NET for background jobs
builder.Services.AddQuartz(q =>
{
    // CleanupAdmissionsJob - runs every day at 3 AM
    var cleanupJobKey = new JobKey("CleanupAdmissionsJob");
    q.AddJob<CleanupAdmissionsJob>(opts => opts.WithIdentity(cleanupJobKey));
    q.AddTrigger(opts => opts
        .ForJob(cleanupJobKey)
        .WithIdentity("CleanupAdmissionsJob-trigger")
        .WithCronSchedule(builder.Configuration["Quartz:CleanupCron"] ?? "0 0 3 * * ?") // Daily at 3 AM
        .WithDescription("Closes applications that have been under review for more than 3 days"));

    // ExternalDataSyncJob - configurable via appsettings
    var syncJobKey = new JobKey("ExternalDataSyncJob");
    q.AddJob<ExternalDataSyncJob>(opts => opts.WithIdentity(syncJobKey));
    q.AddTrigger(opts => opts
        .ForJob(syncJobKey)
        .WithIdentity("ExternalDataSyncJob-trigger")
        .WithCronSchedule(builder.Configuration["Quartz:SyncCron"] ?? "0 0 */6 * * ?") // Every 6 hours
        .WithDescription("Syncs faculty and program data from external API"));
});

builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

// Add Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AdmissionDbContext>("db", tags: new[] { "ready" });

// JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ClockSkew = TimeSpan.Zero,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
});

builder.Services.AddAuthorization();

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Map Health Check endpoints
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");

app.Run();

}
catch (Exception exception)
{
    // NLog: catch setup errors
    logger.Error(exception, "Stopped program because of exception");
    throw;
}
finally
{
    // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
    LogManager.Shutdown();
}
