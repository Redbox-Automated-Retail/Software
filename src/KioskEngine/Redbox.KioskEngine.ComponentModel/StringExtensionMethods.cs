using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using Redbox.JSONPrettyPrinter;

namespace Redbox.KioskEngine.ComponentModel;

public static class StringExtensionMethods
{
	public const string NotAvailable = "N/A";

	public static string ReplaceFirst(this string text, string search, string replace)
	{
		int index = text.IndexOf(search);
		if (index < 0)
		{
			return text;
		}
		return text.Substring(0, index) + replace + text.Substring(index + search.Length);
	}

	public static string ToPrettyJson(this string source)
	{
		if (source == null)
		{
			return string.Empty;
		}
		return PrettyPrinter.GetPrettyString(source);
	}

	public static string CompressString(this string text)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(text);
		MemoryStream memoryStream = new MemoryStream();
		using (GZipStream gZipStream = new GZipStream(memoryStream, CompressionMode.Compress, leaveOpen: true))
		{
			gZipStream.Write(bytes, 0, bytes.Length);
		}
		memoryStream.Position = 0L;
		byte[] buffer = new byte[memoryStream.Length];
		memoryStream.Read(buffer, 0, buffer.Length);
		byte[] compressed = new byte[buffer.Length + 4];
		Buffer.BlockCopy(buffer, 0, compressed, 4, buffer.Length);
		Buffer.BlockCopy(BitConverter.GetBytes(bytes.Length), 0, compressed, 0, 4);
		return Convert.ToBase64String(compressed);
	}

	public static string DecompressString(this string compressedText)
	{
		byte[] decoded = Convert.FromBase64String(compressedText);
		using MemoryStream memoryStream = new MemoryStream();
		int length = BitConverter.ToInt32(decoded, 0);
		memoryStream.Write(decoded, 4, decoded.Length - 4);
		byte[] buffer = new byte[length];
		memoryStream.Position = 0L;
		using (GZipStream gZipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
		{
			gZipStream.Read(buffer, 0, buffer.Length);
		}
		return Encoding.UTF8.GetString(buffer);
	}

	public static string CheckNotAvailable(this string source)
	{
		if (!string.IsNullOrEmpty(source))
		{
			return source;
		}
		return "N/A";
	}

	public static string StoreNumberString(this IMachineSettingsStore source)
	{
		return source.GetValue("Store", "ID", "N/A");
	}
}
