namespace AdmissionService.Entities;

public class Faculty
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Navigation properties
    public ICollection<EducationProgram> EducationPrograms { get; set; } = new List<EducationProgram>();
    public ICollection<Manager> Managers { get; set; } = new List<Manager>();
}
