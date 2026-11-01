using System;
using System.IO;
using System.Security.Cryptography;

namespace Redbox.Core;

internal static class Redbox_002EMacros1309_002EStreamExtensions
{
	public static string ToASCIISHA1Hash(this Stream inputStream)
	{
		SHA1 sHA = SHA1.Create();
		string result = BitConverter.ToString(sHA.ComputeHash(inputStream)).Replace("-", string.Empty);
		if (inputStream.CanSeek)
		{
			inputStream.Seek(0L, SeekOrigin.Begin);
		}
		return result;
	}

	public static byte[] GetBytes(this Stream stream)
	{
		if (stream == null)
		{
			return null;
		}
		if (stream.CanSeek)
		{
			stream.Seek(0L, SeekOrigin.Begin);
		}
		byte[] buffer = new byte[stream.Length];
		int offset = 0;
		int bytesRead;
		while ((bytesRead = stream.Read(buffer, offset, buffer.Length - offset)) > 0)
		{
			offset += bytesRead;
		}
		return buffer;
	}
}
