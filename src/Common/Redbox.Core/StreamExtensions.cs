using System;
using System.IO;
using System.Security.Cryptography;

namespace Redbox.Core;

public static class StreamExtensions
{
	public static string ToASCIISHA1Hash(this Stream inputStream)
	{
		string result = BitConverter.ToString(SHA1.Create().ComputeHash(inputStream)).Replace("-", string.Empty);
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
