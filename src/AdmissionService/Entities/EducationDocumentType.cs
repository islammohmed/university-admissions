namespace AdmissionService.Entities;

/// <summary>
/// Represents a type of education document.
/// Has two relationships with EducationLevel:
/// 1) BelongsToLevel - which education level this document type belongs to
/// 2) NextAvailableLevels - which education levels are available for further study
/// </summary>
public class EducationDocumentType
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// The education level this document type belongs to
    /// (e.g., "Bachelor Diploma" belongs to "Bachelor" level)
    /// </summary>
    public Guid BelongsToLevelId { get; set; }

    // Navigation properties
    /// <summary>
    /// The education level this document type belongs to
    /// </summary>
    public EducationLevel BelongsToLevel { get; set; } = null!;

    /// <summary>
    /// The education levels available for further study with this document
    /// (e.g., Bachelor Diploma allows admission to Master programs)
    /// </summary>
    public ICollection<EducationLevel> NextAvailableLevels { get; set; } = new List<EducationLevel>();

    public ICollection<EducationDocument> EducationDocuments { get; set; } = new List<EducationDocument>();
}
