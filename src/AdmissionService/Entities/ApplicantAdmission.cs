using Shared.Contracts.Enums;

namespace AdmissionService.Entities;

/// <summary>
/// Represents an admission application submitted by an applicant.
/// Contains all necessary information for processing an admission.
/// </summary>
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
    
    /// <summary>
    /// Programs selected by applicant with priorities
    /// </summary>
    public ICollection<AdmissionProgram> AdmissionPrograms { get; set; } = new List<AdmissionProgram>();
}
