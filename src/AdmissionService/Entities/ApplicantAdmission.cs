using Shared.Contracts.Enums;

namespace AdmissionService.Entities;

public class ApplicantAdmission
{
    public Guid Id { get; set; }
    public Guid ApplicantId { get; set; }
    public Guid? ManagerId { get; set; }
    public Guid EducationProgramId { get; set; }
    public AdmissionStatus Status { get; set; } = AdmissionStatus.Created;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public Applicant Applicant { get; set; } = null!;
    public Manager? Manager { get; set; }
    public EducationProgram EducationProgram { get; set; } = null!;
}
