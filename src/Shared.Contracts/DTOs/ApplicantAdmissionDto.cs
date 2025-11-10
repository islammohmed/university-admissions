using Shared.Contracts.Enums;

namespace Shared.Contracts.DTOs;

public class ApplicantAdmissionDto
{
    public Guid Id { get; set; }
    public Guid ApplicantId { get; set; }
    public string ApplicantFullName { get; set; } = string.Empty;
    public Guid? ManagerId { get; set; }
    public string? ManagerFullName { get; set; }
    public Guid EducationProgramId { get; set; }
    public string EducationProgramName { get; set; } = string.Empty;
    public AdmissionStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
