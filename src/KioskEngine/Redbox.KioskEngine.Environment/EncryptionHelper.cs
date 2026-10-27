using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Redbox.KioskEngine.Environment;

public static class EncryptionHelper
{
	private static readonly byte[] m_keyValue = new Guid("{02CAD7C4-E0F5-4b1b-BD04-808B840A61D2}").ToByteArray();

	private static readonly byte[] m_initialVector = new Guid("{AD186FCD-2ACD-41a8-958D-6CA7DAEB1DE5}").ToByteArray();

	public static string Encrypt(string data)
	{
		if (data == null)
		{
			return null;
		}
		byte[] bytes = Encoding.Default.GetBytes(data);
		using TripleDESCryptoServiceProvider tripleDESCryptoServiceProvider = new TripleDESCryptoServiceProvider();
		using ICryptoTransform transform = tripleDESCryptoServiceProvider.CreateEncryptor(m_keyValue, m_initialVector);
		using MemoryStream memoryStream = new MemoryStream();
		CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write);
		cryptoStream.Write(bytes, 0, bytes.Length);
		cryptoStream.FlushFinalBlock();
		return Convert.ToBase64String(memoryStream.ToArray());
	}
}
