namespace AdmissionService.Entities;

public class EducationProgram
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string EducationLanguage { get; set; } = string.Empty;
    public string EducationForm { get; set; } = string.Empty;
    public Guid FacultyId { get; set; }
    public Guid EducationLevelId { get; set; }

    // Navigation properties
    public Faculty Faculty { get; set; } = null!;
    public EducationLevel EducationLevel { get; set; } = null!;
    public ICollection<ApplicantAdmission> ApplicantAdmissions { get; set; } = new List<ApplicantAdmission>();
    public ICollection<AdmissionProgram> AdmissionPrograms { get; set; } = new List<AdmissionProgram>();
}
