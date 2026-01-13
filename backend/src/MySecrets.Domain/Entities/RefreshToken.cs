using MySecrets.Domain.Common;

namespace MySecrets.Domain.Entities;

public class RefreshToken : BaseEntity
{
    public Guid UserId { get; set; }
    
    /// <summary>
    /// SHA256 hash of the actual refresh token.
    /// The plain token is never stored - only its hash.
    /// </summary>
    public string TokenHash { get; set; } = string.Empty;
    
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? RevokedReason { get; set; }
    public string? ReplacedByTokenHash { get; set; }
    public string CreatedByIp { get; set; } = string.Empty;
    public string? RevokedByIp { get; set; }
    
    // Navigation property
    public virtual User User { get; set; } = null!;
    
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive => !IsRevoked && !IsExpired;
}
