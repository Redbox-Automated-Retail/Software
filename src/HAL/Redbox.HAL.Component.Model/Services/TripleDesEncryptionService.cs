using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Redbox.HAL.Component.Model.Services;

public sealed class TripleDesEncryptionService : IEncryptionService
{
	private readonly byte[] m_keyValue = new Guid("{776DA6AF-3033-43ee-B379-2D4F28B5F1FC}").ToByteArray();

	private readonly byte[] m_initialVector = new Guid("{F375D7E0-4572-4518-9C2F-E8F022F42AA7}").ToByteArray();

	public byte[] Encrypt(byte[] inputArray)
	{
		using TripleDESCryptoServiceProvider tripleDESCryptoServiceProvider = new TripleDESCryptoServiceProvider();
		ICryptoTransform transform = tripleDESCryptoServiceProvider.CreateEncryptor(m_keyValue, m_initialVector);
		return DoTransform(transform, inputArray);
	}

	public byte[] Decrypt(byte[] inputArray)
	{
		using TripleDESCryptoServiceProvider tripleDESCryptoServiceProvider = new TripleDESCryptoServiceProvider();
		ICryptoTransform transform = tripleDESCryptoServiceProvider.CreateDecryptor(m_keyValue, m_initialVector);
		return DoTransform(transform, inputArray);
	}

	public string EncryptToBase64(string source)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(source);
		return Convert.ToBase64String(Encrypt(bytes));
	}

	public string DecryptFromBase64(string source)
	{
		byte[] inputArray = Convert.FromBase64String(source);
		return Encoding.UTF8.GetString(Decrypt(inputArray));
	}

	public string HashFile(string fullFilePath)
	{
		using SHA1CryptoServiceProvider algorithm = new SHA1CryptoServiceProvider();
		return HashFile(algorithm, fullFilePath);
	}

	public string HashFile(HashAlgorithm algorithm, string fullFilePath)
	{
		if (!File.Exists(fullFilePath))
		{
			throw new ArgumentException("fullFilePath doesn't exist.");
		}
		byte[] array = null;
		using (FileStream inputStream = new FileStream(fullFilePath, FileMode.Open))
		{
			array = algorithm.ComputeHash(inputStream);
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < array.Length; i++)
		{
			stringBuilder.Append(array[i].ToString("x2"));
		}
		return stringBuilder.ToString();
	}

	private byte[] DoTransform(ICryptoTransform transform, byte[] inputArray)
	{
		using MemoryStream memoryStream = new MemoryStream();
		CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write);
		cryptoStream.Write(inputArray, 0, inputArray.Length);
		cryptoStream.FlushFinalBlock();
		return memoryStream.ToArray();
	}
}
