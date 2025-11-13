namespace Shared.Contracts.DTOs;

public class SendNotificationDto
{
    public string Email { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? ApplicantId { get; set; }
}
