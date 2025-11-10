namespace AdmissionService.Entities;

public class EducationDocument : Document
{
    public string Name { get; set; } = string.Empty;
    public Guid EducationDocumentTypeId { get; set; }

    // Navigation properties
    public EducationDocumentType EducationDocumentType { get; set; } = null!;
}
