using Shared.Contracts.Enums;

namespace AdmissionService.Entities;

public class Applicant
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public Gender Gender { get; set; }
    public string Citizenship { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public AdmissionStatus Status { get; set; } = AdmissionStatus.UnderReview;
    public DateTime AppliedAt { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<ApplicantAdmission> ApplicantAdmissions { get; set; } = new List<ApplicantAdmission>();
    public ICollection<Document> Documents { get; set; } = new List<Document>();
}
