using System.Threading.Tasks;
using UpdateClientService.API.Services.IoT.Certificate.Security;

namespace UpdateClientService.API.Services.IoT.Security.Certificate;

public class SecurityService : ISecurityService
{
	private readonly IHashService _hashService;

	private readonly IEncryptionService _encryptionService;

	public SecurityService(IHashService hashService, IEncryptionService encryptionService)
	{
		_hashService = hashService;
		_encryptionService = encryptionService;
	}

	public async Task<string> GetIoTCertServicePassword(string kioskId)
	{
		return await _hashService.GetKioskPassword(kioskId);
	}

	public async Task<string> GetCertificatePassword(string kioskId)
	{
		return await _hashService.GetCertificatePassword(kioskId);
	}

	public async Task<string> Encrypt(string plainText)
	{
		return await _encryptionService.Encrypt(plainText);
	}
}
