using Microsoft.AspNetCore.Identity;
using Shared.Contracts.Enums;

namespace IdentityService.Models;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
