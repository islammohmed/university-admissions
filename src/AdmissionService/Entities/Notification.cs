namespace AdmissionService.Entities;

public class Notification
{
    public Guid Id { get; set; }
    public string Message { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsSent { get; set; } = false;
    public DateTime? SentAt { get; set; }
}
