namespace AdmissionService.Entities;

/// <summary>
/// Abstract base class for applicant documents.
/// Represents a personal document (passport, education document, etc.)
/// and references the physical file scan/copy.
/// </summary>
public abstract class Document
{
    public Guid Id { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public Guid ApplicantId { get; set; }
    public Guid FileId { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Applicant Applicant { get; set; } = null!;
    public File File { get; set; } = null!;
}
