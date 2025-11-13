using Shared.Contracts.Enums;

namespace AdmissionService.Entities;

/// <summary>
/// Represents a university employee responsible for moderating the admission process.
/// Two types: Faculty Manager (works with specific faculty) and Head Manager (oversees entire campaign)
/// </summary>
public class Manager
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Type of manager: FacultyManager or HeadManager
    /// </summary>
    public ManagerType ManagerType { get; set; } = ManagerType.FacultyManager;
    
    /// <summary>
    /// Faculty assignment for Faculty Managers (null for Head Managers)
    /// </summary>
    public Guid? FacultyId { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Faculty? Faculty { get; set; }
    public ICollection<ApplicantAdmission> ApplicantAdmissions { get; set; } = new List<ApplicantAdmission>();
}
