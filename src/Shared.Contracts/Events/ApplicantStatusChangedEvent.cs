namespace Shared.Contracts.Events;

public class ApplicantStatusChangedEvent
{
    public Guid ApplicantId { get; set; }
    public string OldStatus { get; set; } = string.Empty;
    public string NewStatus { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
    public string ApplicantEmail { get; set; } = string.Empty;
    public string ApplicantName { get; set; } = string.Empty;
}
