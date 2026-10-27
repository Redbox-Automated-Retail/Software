using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using UpdateClientService.API.Services.Configuration;

namespace UpdateClientService.API.Services.IoT.Certificate.Security;

public class EncryptionService : IEncryptionService
{
	private readonly int _saltSize = 32;

	private readonly string _key = ",$=Aqy*)eChz+62ySJUTRX\\j5.hjeCO;8p*R+(90LQvCg?-K(}+at7'ns^IvL,CM+4;Dk3}Pt7@~ai(6u}Ub6Eg^tsl:KEB@&yX+,SK?:$6h[V4hwXEY#*|Oe5G9J6tmvNRDu*Gs4]lRzN4\\mzJkZ&?IOipWJZ6,DpXFh?t\"%LEb+At&V'Iwx|[w}R!.M`L6`{|q#u@o.@]16,v\\tmxe2\\\\[3o-UzFlEYV:We>5qq>eT7`]@c8$mVq;SELkHU3q\"R)x?XtFU\\B@<$qX;IE_'bSCsbf3ezF<'0<w}uQ(L0P*/x\"#:2<V![z0n'I;alt#8`<V)J];7__lNhD@?kD\\gzFI+GrmYsqT)\"`U[T(5/b$KKumUb+G+|>fe)IGQFaf^`X<`0ap-+cd_t{q8/weN6n/Jdqmu8*6EC7U{$[+3quaAiADOMz4k@d2yJ,Nv<pE=X`R^3.%WwZ|%)ge5[E@YBF1Eul9$w\"fm0Lu-7Jds{O?XDJ>'pUW[A";

	public async Task<string> Encrypt(ConfigurationEncryptionType encryptionType, string plainText)
	{
		if (encryptionType == ConfigurationEncryptionType.EncryptType1)
		{
			return await Encrypt(plainText);
		}
		throw new ArgumentException($"Invalid encryptionType value: {encryptionType}");
	}

	public async Task<string> Encrypt(string plainText)
	{
		if (string.IsNullOrEmpty(plainText))
		{
			throw new ArgumentNullException("plainText");
		}
		if (string.IsNullOrEmpty(_key))
		{
			throw new ArgumentNullException("key");
		}
		using Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(_key, _saltSize);
		byte[] array = rfc2898DeriveBytes.Salt;
		byte[] bytes = rfc2898DeriveBytes.GetBytes(32);
		byte[] bytes2 = rfc2898DeriveBytes.GetBytes(16);
		using AesManaged aesManaged = new AesManaged();
		using ICryptoTransform transform = aesManaged.CreateEncryptor(bytes, bytes2);
		using MemoryStream memoryStream = new MemoryStream();
		using (CryptoStream stream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write))
		{
			using StreamWriter streamWriter = new StreamWriter(stream);
			streamWriter.Write(plainText);
		}
		byte[] array2 = memoryStream.ToArray();
		Array.Resize(ref array, array.Length + array2.Length);
		Array.Copy(array2, 0, array, _saltSize, array2.Length);
		return Convert.ToBase64String(array);
	}

	public async Task<string> Decrypt(ConfigurationEncryptionType encryptionType, string cipherText)
	{
		if (encryptionType == ConfigurationEncryptionType.EncryptType1)
		{
			return await Decrypt(cipherText);
		}
		throw new ArgumentException($"Invalid encryptionType value: {encryptionType}");
	}

	public async Task<string> Decrypt(string ciphertext)
	{
		if (string.IsNullOrEmpty(ciphertext))
		{
			throw new ArgumentNullException("cipherText");
		}
		if (string.IsNullOrEmpty(_key))
		{
			throw new ArgumentNullException("key");
		}
		byte[] array = Convert.FromBase64String(ciphertext);
		byte[] salt = array.Take(_saltSize).ToArray();
		byte[] buffer = array.Skip(_saltSize).Take(array.Length - _saltSize).ToArray();
		using Rfc2898DeriveBytes keyDerivationFunction = new Rfc2898DeriveBytes(_key, salt);
		byte[] bytes = keyDerivationFunction.GetBytes(32);
		byte[] bytes2 = keyDerivationFunction.GetBytes(16);
		using AesManaged aesManaged = new AesManaged();
		using ICryptoTransform decryptor = aesManaged.CreateDecryptor(bytes, bytes2);
		using MemoryStream memoryStream = new MemoryStream(buffer);
		using CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
		using StreamReader streamReader = new StreamReader(cryptoStream);
		return await streamReader.ReadToEndAsync();
	}
}
