using System;
using System.IO;
using System.Security.Cryptography;

namespace Redbox.Core;

public static class ByteArrayExtensions
{
	private static readonly byte[] m_keyValue = new Guid("{776DA6AF-3033-43ee-B379-2D4F28B5F1FC}").ToByteArray();

	private static readonly byte[] m_initialVector = new Guid("{F375D7E0-4572-4518-9C2F-E8F022F42AA7}").ToByteArray();

	public static void WriteToFile(this byte[] buffer, string fileName)
	{
		if (buffer != null && fileName != null)
		{
			File.WriteAllBytes(fileName, buffer);
		}
	}

	public static byte[] Encrypt(this byte[] inputArray)
	{
		TripleDESCryptoServiceProvider tripleDESCryptoServiceProvider = new TripleDESCryptoServiceProvider();
		ICryptoTransform transform = tripleDESCryptoServiceProvider.CreateEncryptor(m_keyValue, m_initialVector);
		using MemoryStream memoryStream = new MemoryStream();
		CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write);
		cryptoStream.Write(inputArray, 0, inputArray.Length);
		cryptoStream.FlushFinalBlock();
		return memoryStream.ToArray();
	}

	public static byte[] Decrypt(this byte[] inputArray)
	{
		TripleDESCryptoServiceProvider tripleDESCryptoServiceProvider = new TripleDESCryptoServiceProvider();
		ICryptoTransform transform = tripleDESCryptoServiceProvider.CreateDecryptor(m_keyValue, m_initialVector);
		using MemoryStream memoryStream = new MemoryStream();
		CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write);
		cryptoStream.Write(inputArray, 0, inputArray.Length);
		cryptoStream.FlushFinalBlock();
		return memoryStream.ToArray();
	}

	public static string ToBase85(this byte[] inputArray)
	{
		return ASCII85.Encode(inputArray);
	}

	public static string ToBase64(this byte[] inputArray)
	{
		return Convert.ToBase64String(inputArray);
	}

	public static string ToASCIISHA1Hash(this byte[] inputArray)
	{
		SHA1 sHA = SHA1.Create();
		byte[] array = sHA.ComputeHash(inputArray);
		return BitConverter.ToString(array).Replace("-", string.Empty);
	}
}
