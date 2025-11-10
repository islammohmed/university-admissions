namespace Shared.Contracts.Events;

public class ApplicantRegisteredEvent
{
    public Guid ApplicantId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime RegisteredAt { get; set; }
}
