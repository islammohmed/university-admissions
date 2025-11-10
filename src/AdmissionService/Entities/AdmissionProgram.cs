namespace AdmissionService.Entities;

public class AdmissionProgram
{
    public Guid Id { get; set; }
    public int Priority { get; set; }
    public Guid EducationProgramId { get; set; }

    // Navigation properties
    public EducationProgram EducationProgram { get; set; } = null!;
}
