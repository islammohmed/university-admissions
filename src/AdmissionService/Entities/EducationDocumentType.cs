namespace AdmissionService.Entities;

public class EducationDocumentType
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // Navigation properties
    public ICollection<EducationDocument> EducationDocuments { get; set; } = new List<EducationDocument>();
}
