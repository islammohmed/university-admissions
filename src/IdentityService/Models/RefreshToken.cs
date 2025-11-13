namespace IdentityService.Models;

public class RefreshToken
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Token { get; set; } = default!;
    public string UserId { get; set; } = default!;
    public DateTime ExpiryDate { get; set; }
    public bool Revoked { get; set; } = false;
    public string? DeviceInfo { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid? ReplacedByTokenId { get; set; }
    
    // Navigation property
    public ApplicationUser User { get; set; } = default!;
}
