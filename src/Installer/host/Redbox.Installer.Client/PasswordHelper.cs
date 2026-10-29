using System;
using System.Security.Cryptography;
using System.Text;

namespace Redbox.Installer.Client;

public static class PasswordHelper
{
	public static byte[] Key => new byte[16]
	{
		94, 55, 65, 92, 46, 131, 78, 211, 167, 183,
		145, 16, 244, 128, 166, 93
	};

	public static CipherMode Mode => CipherMode.ECB;

	public static PaddingMode Padding => PaddingMode.PKCS7;

	public static string Encrypt(string raw)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(raw);
		TripleDESCryptoServiceProvider tripleDESCryptoServiceProvider = new TripleDESCryptoServiceProvider();
		tripleDESCryptoServiceProvider.Key = Key;
		tripleDESCryptoServiceProvider.Mode = Mode;
		tripleDESCryptoServiceProvider.Padding = Padding;
		using TripleDESCryptoServiceProvider tripleDESCryptoServiceProvider2 = tripleDESCryptoServiceProvider;
		ICryptoTransform cryptoTransform = tripleDESCryptoServiceProvider2.CreateEncryptor();
		byte[] array = cryptoTransform.TransformFinalBlock(bytes, 0, bytes.Length);
		return Convert.ToBase64String(array, 0, array.Length);
	}

	public static string Decrypt(string base64encrypted)
	{
		byte[] array = Convert.FromBase64String(base64encrypted);
		TripleDESCryptoServiceProvider tripleDESCryptoServiceProvider = new TripleDESCryptoServiceProvider();
		tripleDESCryptoServiceProvider.Key = Key;
		tripleDESCryptoServiceProvider.Mode = Mode;
		tripleDESCryptoServiceProvider.Padding = Padding;
		using TripleDESCryptoServiceProvider tripleDESCryptoServiceProvider2 = tripleDESCryptoServiceProvider;
		ICryptoTransform cryptoTransform = tripleDESCryptoServiceProvider2.CreateDecryptor();
		byte[] bytes = cryptoTransform.TransformFinalBlock(array, 0, array.Length);
		return Encoding.UTF8.GetString(bytes);
	}
}
