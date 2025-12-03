using System.Security.Cryptography;

namespace TaskFlow.Api.Utilities;

/// <summary>
/// Implements secure password hashing using the PBKDF2 algorithm.
/// This method uses key stretching to defend against brute-force attacks.
/// </summary>
public class PBKDF2PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16; // 16 bytes for salt
    private const int KeySize = 32; // 32 bytes for key
    private const int Iterations = 10000; // Number of iterations

    private static readonly HashAlgorithmName _hashAlgorithmName = HashAlgorithmName.SHA256;

    /// <inheritdoc />
    public string Hash(string password)
    {
        byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);

        byte[] hash = Rfc2898DeriveBytes(password, salt, Iterations, _hashAlgorithmName);

        // Combine salt and hash for storage (e.g., "salt:hash")
        return string.Join(":", Convert.ToBase64String(salt), Convert.ToBase64String(hash));
    }

    /// <inheritdoc />
    public bool Verify(string passwordHash, string password)
    {
        string[] parts = passwordHash.Split(':');

        if (parts.Length != 2)
            return false;

        byte[] salt = Convert.FromBase64String(parts[0]);
        byte[] hash = Convert.FromBase64String(parts[1]);

        byte[] newHash = Rfc2898DeriveBytes(password, salt, Iterations, _hashAlgorithmName);

        // Use CryptographicOperations for constant-time comparison (prevents timing attacks)
        return CryptographicOperations.FixedTimeEquals(newHash, hash);
    }

    private static byte[] Rfc2898DeriveBytes(string password, byte[] salt, int iterations,
        HashAlgorithmName hashAlgorithm)
    {
        return new Rfc2898DeriveBytes(password, salt, iterations, hashAlgorithm).GetBytes(KeySize);
    }
}