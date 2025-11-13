using MassTransit;
using Microsoft.EntityFrameworkCore;
using NotificationService.Data;
using NotificationService.Entities;
using NotificationService.Services;
using Shared.Contracts.Events;

namespace NotificationService.Consumers;

public class ApplicantStatusChangedConsumer : IConsumer<ApplicantStatusChangedEvent>
{
    private readonly NotificationDbContext _context;
    private readonly EmailService _emailService;
    private readonly ILogger<ApplicantStatusChangedConsumer> _logger;

    public ApplicantStatusChangedConsumer(
        NotificationDbContext context,
        EmailService emailService,
        ILogger<ApplicantStatusChangedConsumer> logger)
    {
        _context = context;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ApplicantStatusChangedEvent> context)
    {
        var evt = context.Message;
        _logger.LogInformation(
            "Received ApplicantStatusChangedEvent for Applicant {ApplicantId}: {OldStatus} -> {NewStatus}",
            evt.ApplicantId, evt.OldStatus, evt.NewStatus);

        try
        {
            // 1) Create email body
            var subject = $"Your Application Status Has Changed";
            var body = GenerateEmailBody(evt);

            // 2) Create notification record
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                To = evt.ApplicantEmail,
                Subject = subject,
                Body = body,
                ApplicantId = evt.ApplicantId.ToString(),
                CreatedAt = DateTime.UtcNow,
                Status = "Pending"
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            // 3) Send email via SMTP/SendGrid
            var success = await _emailService.SendEmailAsync(evt.ApplicantEmail, subject, body);

            // 4) Update notification status
            if (success)
            {
                notification.Status = "Sent";
                notification.SentAt = DateTime.UtcNow;
                _logger.LogInformation(
                    "Successfully sent notification to {Email} for status change", evt.ApplicantEmail);
            }
            else
            {
                notification.Status = "Failed";
                notification.ErrorMessage = "Failed to send email";
                _logger.LogWarning(
                    "Failed to send notification to {Email} for status change", evt.ApplicantEmail);
            }

            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing ApplicantStatusChangedEvent for Applicant {ApplicantId}",
                evt.ApplicantId);
            throw; // Re-throw to let MassTransit handle retry logic
        }
    }

    private string GenerateEmailBody(ApplicantStatusChangedEvent evt)
    {
        return $@"
            <html>
            <body>
                <h2>Dear {evt.ApplicantName},</h2>
                <p>Your application status has been updated.</p>
                <p><strong>Previous Status:</strong> {evt.OldStatus}</p>
                <p><strong>New Status:</strong> {evt.NewStatus}</p>
                <p><strong>Updated At:</strong> {evt.Timestamp:yyyy-MM-dd HH:mm:ss} UTC</p>
                <p>Please log in to your account for more details.</p>
                <br/>
                <p>Best regards,<br/>University Admissions Team</p>
            </body>
            </html>
        ";
    }
}
