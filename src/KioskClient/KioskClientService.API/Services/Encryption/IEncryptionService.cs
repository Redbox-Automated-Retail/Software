using System.Threading.Tasks;

namespace KioskClientService.API.Services.Encryption;

public interface IEncryptionService
{
	Task<string> Encrypt(string plainText);

	Task<string> Encrypt(ConfigurationEncryptionType encryptionType, string plainText);

	Task<string> Decrypt(string ciphertext);

	Task<string> Decrypt(ConfigurationEncryptionType encryptionType, string cipherText);
}
