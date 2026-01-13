namespace MySecrets.Domain.Interfaces;

/// <summary>
/// Service for encrypting and decrypting secrets using AES-256-GCM.
/// Each user has a derived encryption key based on their user ID.
/// </summary>
public interface IEncryptionService
{
    /// <summary>
    /// Encrypts a plaintext password using AES-256-GCM with a user-specific key.
    /// </summary>
    /// <param name="plainText">The password to encrypt</param>
    /// <param name="userId">User ID for key derivation</param>
    /// <returns>Base64-encoded encrypted data (IV + ciphertext + auth tag)</returns>
    string Encrypt(string plainText, Guid userId);
    
    /// <summary>
    /// Decrypts an encrypted password using AES-256-GCM with a user-specific key.
    /// </summary>
    /// <param name="encryptedData">Base64-encoded encrypted data</param>
    /// <param name="userId">User ID for key derivation</param>
    /// <returns>The decrypted plaintext password</returns>
    string Decrypt(string encryptedData, Guid userId);
}
