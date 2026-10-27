using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Redbox.NetCore.Middleware.Extensions;

public static class EncryptionHelper
{
	private static readonly byte[] m_keyValue = new Guid("{02CAD7C4-E0F5-4b1b-BD04-808B840A61D2}").ToByteArray();

	private static readonly byte[] m_initialVector = new Guid("{AD186FCD-2ACD-41a8-958D-6CA7DAEB1DE5}").ToByteArray();

	public static string Decrypt(string data)
	{
		byte[] array = Convert.FromBase64String(data);
		TripleDESCryptoServiceProvider tripleDESCryptoServiceProvider = new TripleDESCryptoServiceProvider();
		try
		{
			using ICryptoTransform transform = ((SymmetricAlgorithm)(object)tripleDESCryptoServiceProvider).CreateDecryptor(m_keyValue, m_initialVector);
			using MemoryStream memoryStream = new MemoryStream();
			using CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write);
			cryptoStream.Write(array, 0, array.Length);
			cryptoStream.FlushFinalBlock();
			return Encoding.Default.GetString(memoryStream.ToArray());
		}
		finally
		{
			((IDisposable)(object)tripleDESCryptoServiceProvider)?.Dispose();
		}
	}

	public static string Encrypt(string data)
	{
		byte[] bytes = Encoding.Default.GetBytes(data);
		TripleDESCryptoServiceProvider tripleDESCryptoServiceProvider = new TripleDESCryptoServiceProvider();
		try
		{
			using ICryptoTransform transform = ((SymmetricAlgorithm)(object)tripleDESCryptoServiceProvider).CreateEncryptor(m_keyValue, m_initialVector);
			using MemoryStream memoryStream = new MemoryStream();
			using CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write);
			cryptoStream.Write(bytes, 0, bytes.Length);
			cryptoStream.FlushFinalBlock();
			return Convert.ToBase64String(memoryStream.ToArray());
		}
		finally
		{
			((IDisposable)(object)tripleDESCryptoServiceProvider)?.Dispose();
		}
	}
}
