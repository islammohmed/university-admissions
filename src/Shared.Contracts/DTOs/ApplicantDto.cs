using Shared.Contracts.Enums;

namespace Shared.Contracts.DTOs;

public class ApplicantDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }
    public Gender Gender { get; set; }
    public string Citizenship { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
}
