using System.Security.Cryptography;

namespace Redbox.HAL.Component.Model;

public interface IEncryptionService
{
	byte[] Encrypt(byte[] inputArray);

	byte[] Decrypt(byte[] inputArray);

	string EncryptToBase64(string source);

	string DecryptFromBase64(string source);

	string HashFile(string fullFilePath);

	string HashFile(HashAlgorithm algorithm, string fullFilePath);
}
