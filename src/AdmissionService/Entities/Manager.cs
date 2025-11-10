namespace AdmissionService.Entities;

public class Manager
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Guid? FacultyId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Faculty? Faculty { get; set; }
    public ICollection<ApplicantAdmission> ApplicantAdmissions { get; set; } = new List<ApplicantAdmission>();
}
