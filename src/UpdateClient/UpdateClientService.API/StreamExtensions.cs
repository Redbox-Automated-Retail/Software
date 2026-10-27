using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace UpdateClientService.API;

public static class StreamExtensions
{
	public static async Task<string> GetSHA1HashAsync(this Stream stream)
	{
		using MemoryStream ms = new MemoryStream();
		await stream.CopyToAsync(ms);
		byte[] buffer = ms.ToArray();
		using SHA1Managed sHA1Managed = new SHA1Managed();
		byte[] inArray = sHA1Managed.ComputeHash(buffer);
		return Convert.ToBase64String(inArray);
	}

	public static string GetSHA1Hash(this Stream stream)
	{
		using MemoryStream memoryStream = new MemoryStream();
		stream.CopyTo(memoryStream);
		byte[] buffer = memoryStream.ToArray();
		using SHA1Managed sHA1Managed = new SHA1Managed();
		byte[] inArray = sHA1Managed.ComputeHash(buffer);
		return Convert.ToBase64String(inArray);
	}

	public static string ReadToEnd(this Stream stream)
	{
		string text = null;
		using StreamReader streamReader = new StreamReader(stream);
		return streamReader.ReadToEnd();
	}

	public static byte[] GetBytes(this Stream stream)
	{
		using MemoryStream memoryStream = new MemoryStream();
		stream.CopyTo(memoryStream);
		return memoryStream.ToArray();
	}
}
