using MySecrets.Domain.Common;

namespace MySecrets.Domain.Entities;

public class Secret : BaseEntity
{
    public Guid UserId { get; set; }
    public string WebsiteUrl { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    
    /// <summary>
    /// Encrypted password stored as Base64 string.
    /// Format: IV (12 bytes) + Ciphertext + AuthTag (16 bytes)
    /// </summary>
    public string EncryptedPassword { get; set; } = string.Empty;
    
    public string? Notes { get; set; }
    public string? Category { get; set; }
    public bool IsFavorite { get; set; }
    
    // Navigation property
    public virtual User User { get; set; } = null!;
}
