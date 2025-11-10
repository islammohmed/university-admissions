using Microsoft.EntityFrameworkCore;
using NotificationService.Data;
using NotificationService.Services;

namespace NotificationService;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceProvider _serviceProvider;

    public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Notification Worker running at: {time}", DateTimeOffset.Now);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingNotifications(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing notifications");
            }

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }

    private async Task ProcessPendingNotifications(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
        var emailService = scope.ServiceProvider.GetRequiredService<EmailService>();

        var pendingNotifications = await dbContext.Notifications
            .Where(n => !n.IsSent && n.RetryCount < 3)
            .OrderBy(n => n.CreatedAt)
            .Take(10)
            .ToListAsync(cancellationToken);

        foreach (var notification in pendingNotifications)
        {
            _logger.LogInformation("Processing notification {Id} for {Email}", 
                notification.Id, notification.UserEmail);

            var success = await emailService.SendEmailAsync(
                notification.UserEmail,
                "University Admissions Notification",
                notification.Message);

            if (success)
            {
                notification.IsSent = true;
                notification.SentAt = DateTime.UtcNow;
                _logger.LogInformation("Successfully sent notification {Id}", notification.Id);
            }
            else
            {
                notification.RetryCount++;
                notification.ErrorMessage = "Failed to send email";
                _logger.LogWarning("Failed to send notification {Id}, retry count: {RetryCount}", 
                    notification.Id, notification.RetryCount);
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        if (pendingNotifications.Any())
        {
            _logger.LogInformation("Processed {Count} notifications", pendingNotifications.Count);
        }
    }
}
