namespace AdmissionService.Entities;

public abstract class Document
{
    public Guid Id { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public Guid ApplicantId { get; set; }
    public Guid FileId { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Applicant Applicant { get; set; } = null!;
}
