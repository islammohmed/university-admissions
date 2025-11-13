namespace NotificationService.Entities;

public class Notification
{
    public Guid Id { get; set; }
    public string To { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? ApplicantId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? SentAt { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Sent, Failed
    public int RetryCount { get; set; } = 0;
    public string? ErrorMessage { get; set; }
}
