using System;
using System.Security.Cryptography;
using System.Text;

namespace Redbox.HAL.Core;

public static class ByteHelper
{
	private static readonly byte[] m_keyValue = new Guid("{776DA6AF-3033-43ee-B379-2D4F28B5F1FC}").ToByteArray();

	private static readonly byte[] m_initialVector = new Guid("{F375D7E0-4572-4518-9C2F-E8F022F42AA7}").ToByteArray();

	public static int IndexOf(byte[] array, byte b)
	{
		if (array == null || array.Length == 0)
		{
			return -1;
		}
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] == b)
			{
				return i;
			}
		}
		return -1;
	}

	public static string ToBase64(byte[] inputArray)
	{
		return Convert.ToBase64String(inputArray);
	}

	public static string AsString(byte[] array)
	{
		StringBuilder stringBuilder = new StringBuilder(array.Length * 2);
		foreach (byte b in array)
		{
			stringBuilder.AppendFormat("{0:x2}", b);
		}
		return stringBuilder.ToString();
	}

	internal static byte[] Compress(byte[] buffer, CompressionType type)
	{
		return CompressionAlgorithm.GetAlgorithm(type).Compress(buffer);
	}

	internal static byte[] Decompress(byte[] buffer, CompressionType type)
	{
		return CompressionAlgorithm.GetAlgorithm(type).Decompress(buffer);
	}

	internal static byte[] Encrypt(byte[] inputArray)
	{
		TripleDESCryptoServiceProvider tripleDESCryptoServiceProvider = new TripleDESCryptoServiceProvider();
		ICryptoTransform transform = tripleDESCryptoServiceProvider.CreateEncryptor(m_keyValue, m_initialVector);
		using MemoryStream memoryStream = StreamBase.New();
		CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write);
		cryptoStream.Write(inputArray, 0, inputArray.Length);
		cryptoStream.FlushFinalBlock();
		return memoryStream.ToArray();
	}

	internal static byte[] Decrypt(byte[] inputArray)
	{
		TripleDESCryptoServiceProvider tripleDESCryptoServiceProvider = new TripleDESCryptoServiceProvider();
		ICryptoTransform transform = tripleDESCryptoServiceProvider.CreateDecryptor(m_keyValue, m_initialVector);
		using MemoryStream memoryStream = StreamBase.New();
		CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write);
		cryptoStream.Write(inputArray, 0, inputArray.Length);
		cryptoStream.FlushFinalBlock();
		return memoryStream.ToArray();
	}
}
