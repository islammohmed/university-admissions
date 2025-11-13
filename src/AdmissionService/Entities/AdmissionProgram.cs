namespace AdmissionService.Entities;

/// <summary>
/// Association class that stores data about the program selected by an applicant for admission.
/// Represents the many-to-many relationship between ApplicantAdmission and EducationProgram
/// with additional data (Priority).
/// An applicant can apply to multiple programs with different priorities.
/// </summary>
public class AdmissionProgram
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// Reference to the applicant's admission application
    /// </summary>
    public Guid ApplicantAdmissionId { get; set; }
    
    /// <summary>
    /// Reference to the education program being applied to
    /// </summary>
    public Guid EducationProgramId { get; set; }
    
    /// <summary>
    /// Priority of this program choice (1 = first choice, 2 = second choice, etc.)
    /// </summary>
    public int Priority { get; set; }

    // Navigation properties
    public ApplicantAdmission ApplicantAdmission { get; set; } = null!;
    public EducationProgram EducationProgram { get; set; } = null!;
}
