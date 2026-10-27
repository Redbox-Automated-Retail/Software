using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace UpdateClientService.API.Services.IoT.Certificate.Security;

public class HashService : IHashService
{
	public async Task<string> GetKioskPassword(string kioskId)
	{
		SHA512 sHA = new SHA512Managed();
		byte[] inArray = sHA.ComputeHash(Encoding.UTF8.GetBytes(kioskId));
		string text = Convert.ToBase64String(inArray);
		Task<string> saltOfTheKioskPassword = GetSaltOfTheKioskPassword(kioskId);
		inArray = sHA.ComputeHash(Encoding.UTF8.GetBytes(text + saltOfTheKioskPassword));
		return Convert.ToBase64String(inArray);
	}

	public async Task<string> GetCertificatePassword(string kioskId)
	{
		SHA512 sHA = new SHA512Managed();
		byte[] inArray = sHA.ComputeHash(Encoding.UTF8.GetBytes(kioskId));
		string text = Convert.ToBase64String(inArray);
		Task<string> saltOfTheCertificatePassword = GetSaltOfTheCertificatePassword(kioskId);
		inArray = sHA.ComputeHash(Encoding.UTF8.GetBytes(text + saltOfTheCertificatePassword));
		return Convert.ToBase64String(inArray);
	}

	private async Task<string> GetSaltOfTheCertificatePassword(string kioskId)
	{
		int num = 0;
		char c = kioskId[kioskId.Length - 1];
		string text = kioskId + kioskId;
		foreach (char c2 in text)
		{
			num *= c2 * c;
		}
		return num.ToString();
	}

	private async Task<string> GetSaltOfTheKioskPassword(string kioskId)
	{
		int num = 0;
		foreach (char c in kioskId)
		{
			num *= c;
		}
		return num.ToString();
	}
}
