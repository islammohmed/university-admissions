namespace IdentityService.DTOs;

public class AuthResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public int ExpiresIn { get; set; } // Seconds until access token expires
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    
    // Legacy support (can be removed later)
    public string Token 
    { 
        get => AccessToken; 
        set => AccessToken = value; 
    }
    public DateTime ExpiresAt => DateTime.UtcNow.AddSeconds(ExpiresIn);
}
