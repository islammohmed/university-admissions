namespace AdmissionService.Entities;

/// <summary>
/// Represents metadata about a physical file (scan/copy of a document)
/// stored on disk or cloud storage.
/// </summary>
public class File
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string StorageLocation { get; set; } = string.Empty; // Local, Azure, AWS, etc.
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<Document> Documents { get; set; } = new List<Document>();
}
