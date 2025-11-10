namespace AdmissionService.Entities;

public class Passport : Document
{
    public string SeriesNumber { get; set; } = string.Empty;
    public string PlaceOfBirth { get; set; } = string.Empty;
    public DateTime IssuedDate { get; set; }
    public string IssuedBy { get; set; } = string.Empty;
}
