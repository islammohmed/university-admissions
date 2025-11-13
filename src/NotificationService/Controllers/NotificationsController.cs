using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NotificationService.Data;
using NotificationService.Entities;
using NotificationService.Services;
using Shared.Contracts.DTOs;

namespace NotificationService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly NotificationDbContext _context;
    private readonly EmailService _emailService;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(
        NotificationDbContext context,
        EmailService emailService,
        ILogger<NotificationsController> logger)
    {
        _context = context;
        _emailService = emailService;
        _logger = logger;
    }

    /// <summary>
    /// Send a notification via HTTP POST
    /// </summary>
    [HttpPost("send")]
    public async Task<IActionResult> SendNotification([FromBody] SendNotificationDto request)
    {
        try
        {
            // Create notification record
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                To = request.Email,
                Subject = request.Subject,
                Body = request.Body,
                ApplicantId = request.ApplicantId,
                CreatedAt = DateTime.UtcNow,
                Status = "Pending"
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            // Attempt to send email immediately
            var success = await _emailService.SendEmailAsync(request.Email, request.Subject, request.Body);

            if (success)
            {
                notification.Status = "Sent";
                notification.SentAt = DateTime.UtcNow;
                _logger.LogInformation("Notification sent successfully to {Email}", request.Email);
            }
            else
            {
                notification.Status = "Failed";
                notification.ErrorMessage = "Failed to send email";
                _logger.LogWarning("Failed to send notification to {Email}", request.Email);
            }

            await _context.SaveChangesAsync();

            return Ok(new { 
                notificationId = notification.Id, 
                status = notification.Status 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification");
            return StatusCode(500, new { error = "Failed to send notification" });
        }
    }

    /// <summary>
    /// Get notification history for an applicant
    /// </summary>
    [HttpGet("applicant/{applicantId}")]
    public async Task<IActionResult> GetNotificationsByApplicant(string applicantId)
    {
        var notifications = await _context.Notifications
            .Where(n => n.ApplicantId == applicantId)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                Message = n.Body,
                UserId = n.ApplicantId ?? string.Empty,
                UserEmail = n.To,
                CreatedAt = n.CreatedAt,
                IsSent = n.Status == "Sent"
            })
            .ToListAsync();

        return Ok(notifications);
    }

    /// <summary>
    /// Get all notifications
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllNotifications([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var notifications = await _context.Notifications
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(n => new
            {
                n.Id,
                n.To,
                n.Subject,
                n.Status,
                n.CreatedAt,
                n.SentAt
            })
            .ToListAsync();

        return Ok(notifications);
    }
}
