using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using MySecrets.Domain.Interfaces;

namespace MySecrets.Infrastructure.Services;

public class EncryptionSettings
{
    public string MasterKey { get; set; } = string.Empty;
}

/// <summary>
/// AES-256-GCM encryption service with per-user key derivation.
/// 
/// Security Properties:
/// - AES-256: 256-bit key, quantum-resistant for foreseeable future
/// - GCM mode: Provides authenticated encryption (confidentiality + integrity)
/// - HKDF: Derives user-specific keys from master key
/// - Unique IV: Random 96-bit IV per encryption
/// 
/// Storage Format: Base64(IV[12] || Ciphertext || AuthTag[16])
/// </summary>
public class EncryptionService : IEncryptionService
{
    private const int IvSizeBytes = 12;   // 96 bits for GCM
    private const int TagSizeBytes = 16;  // 128 bits for GCM
    private const int KeySizeBytes = 32;  // 256 bits for AES-256

    private readonly byte[] _masterKey;

    public EncryptionService(IOptions<EncryptionSettings> settings)
    {
        if (string.IsNullOrEmpty(settings.Value.MasterKey))
        {
            throw new InvalidOperationException(
                "Encryption master key is not configured. Set ENCRYPTION_MASTER_KEY environment variable.");
        }

        // Master key should be a 64-char hex string (32 bytes)
        _masterKey = Convert.FromHexString(settings.Value.MasterKey);

        if (_masterKey.Length != KeySizeBytes)
        {
            throw new InvalidOperationException(
                $"Master key must be exactly {KeySizeBytes * 2} hex characters ({KeySizeBytes} bytes).");
        }
    }

    public string Encrypt(string plainText, Guid userId)
    {
        if (string.IsNullOrEmpty(plainText))
        {
            throw new ArgumentException("Plain text cannot be null or empty", nameof(plainText));
        }

        var userKey = DeriveUserKey(userId);
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        
        // Generate random IV
        var iv = RandomNumberGenerator.GetBytes(IvSizeBytes);
        
        // Prepare output buffer: IV + Ciphertext + Tag
        var ciphertext = new byte[plainBytes.Length];
        var tag = new byte[TagSizeBytes];

        using var aes = new AesGcm(userKey, TagSizeBytes);
        aes.Encrypt(iv, plainBytes, ciphertext, tag);

        // Combine: IV || Ciphertext || Tag
        var result = new byte[IvSizeBytes + ciphertext.Length + TagSizeBytes];
        Buffer.BlockCopy(iv, 0, result, 0, IvSizeBytes);
        Buffer.BlockCopy(ciphertext, 0, result, IvSizeBytes, ciphertext.Length);
        Buffer.BlockCopy(tag, 0, result, IvSizeBytes + ciphertext.Length, TagSizeBytes);

        return Convert.ToBase64String(result);
    }

    public string Decrypt(string encryptedData, Guid userId)
    {
        if (string.IsNullOrEmpty(encryptedData))
        {
            throw new ArgumentException("Encrypted data cannot be null or empty", nameof(encryptedData));
        }

        var userKey = DeriveUserKey(userId);
        var encryptedBytes = Convert.FromBase64String(encryptedData);

        if (encryptedBytes.Length < IvSizeBytes + TagSizeBytes)
        {
            throw new CryptographicException("Invalid encrypted data format");
        }

        // Extract: IV || Ciphertext || Tag
        var iv = new byte[IvSizeBytes];
        var ciphertextLength = encryptedBytes.Length - IvSizeBytes - TagSizeBytes;
        var ciphertext = new byte[ciphertextLength];
        var tag = new byte[TagSizeBytes];

        Buffer.BlockCopy(encryptedBytes, 0, iv, 0, IvSizeBytes);
        Buffer.BlockCopy(encryptedBytes, IvSizeBytes, ciphertext, 0, ciphertextLength);
        Buffer.BlockCopy(encryptedBytes, IvSizeBytes + ciphertextLength, tag, 0, TagSizeBytes);

        var plainBytes = new byte[ciphertextLength];

        using var aes = new AesGcm(userKey, TagSizeBytes);
        aes.Decrypt(iv, ciphertext, tag, plainBytes);

        return Encoding.UTF8.GetString(plainBytes);
    }

    /// <summary>
    /// Derives a user-specific encryption key using HKDF.
    /// This ensures each user's secrets are encrypted with a unique key,
    /// providing defense-in-depth if one user's data is compromised.
    /// </summary>
    private byte[] DeriveUserKey(Guid userId)
    {
        var info = Encoding.UTF8.GetBytes($"my-secrets-user-key-{userId}");
        return HKDF.DeriveKey(HashAlgorithmName.SHA256, _masterKey, KeySizeBytes, info: info);
    }
}
