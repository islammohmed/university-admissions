namespace AdmissionService.Entities;

public class Faculty
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // Navigation properties
    public ICollection<EducationProgram> EducationPrograms { get; set; } = new List<EducationProgram>();
    public ICollection<Manager> Managers { get; set; } = new List<Manager>();
}
