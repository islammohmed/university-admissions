using Shared.Contracts.Enums;

namespace Shared.Contracts.Events;

public class AdmissionStatusChangedEvent
{
    public Guid AdmissionId { get; set; }
    public Guid ApplicantId { get; set; }
    public string ApplicantEmail { get; set; } = string.Empty;
    public AdmissionStatus OldStatus { get; set; }
    public AdmissionStatus NewStatus { get; set; }
    public DateTime ChangedAt { get; set; }
    public string? ManagerEmail { get; set; }
}
