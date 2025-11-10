namespace Shared.Contracts.DTOs;

public class EducationProgramDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string EducationLanguage { get; set; } = string.Empty;
    public string EducationForm { get; set; } = string.Empty;
    public Guid FacultyId { get; set; }
    public string FacultyName { get; set; } = string.Empty;
    public Guid EducationLevelId { get; set; }
    public string EducationLevelName { get; set; } = string.Empty;
}
