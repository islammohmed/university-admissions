namespace AdmissionService.Entities;

public class EducationLevel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // Navigation properties
    public ICollection<EducationProgram> EducationPrograms { get; set; } = new List<EducationProgram>();
}
