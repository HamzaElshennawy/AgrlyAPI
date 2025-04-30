using System.Security.Cryptography;

namespace AgrlyAPI.Services;

public static class PasswordHashHandler
{
	private const int SaltSize = 16; // 128 bit
	private const int KeySize = 32; // 256 bit
	private const int Iterations = 100000;
	private static readonly HashAlgorithmName HashAlgorithm = HashAlgorithmName.SHA256;
	private const char Delimiter = ';';

	public static string HashPassword(string password)
	{
		var salt = RandomNumberGenerator.GetBytes(SaltSize);
		var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithm, KeySize);

		return string.Join(Delimiter, Convert.ToBase64String(salt), Convert.ToBase64String(hash));
	}

	public static bool VerifyPassword(string password, string hashString)
	{
		var elements = hashString.Split(Delimiter);
		if (elements.Length != 2)
			return false;

		var salt = Convert.FromBase64String(elements[0]);
		var hash = Convert.FromBase64String(elements[1]);

		var hashToCompare = Rfc2898DeriveBytes.Pbkdf2(
			password,
			salt,
			Iterations,
			HashAlgorithm,
			KeySize
		);

		return CryptographicOperations.FixedTimeEquals(hash, hashToCompare);
	}
}
