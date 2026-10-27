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
		int num = text.IndexOf(search);
		if (num < 0)
		{
			return text;
		}
		return text.Substring(0, num) + replace + text.Substring(num + search.Length);
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
		byte[] array = new byte[memoryStream.Length];
		memoryStream.Read(array, 0, array.Length);
		byte[] array2 = new byte[array.Length + 4];
		Buffer.BlockCopy(array, 0, array2, 4, array.Length);
		Buffer.BlockCopy(BitConverter.GetBytes(bytes.Length), 0, array2, 0, 4);
		return Convert.ToBase64String(array2);
	}

	public static string DecompressString(this string compressedText)
	{
		byte[] array = Convert.FromBase64String(compressedText);
		using MemoryStream memoryStream = new MemoryStream();
		int num = BitConverter.ToInt32(array, 0);
		memoryStream.Write(array, 4, array.Length - 4);
		byte[] array2 = new byte[num];
		memoryStream.Position = 0L;
		using (GZipStream gZipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
		{
			gZipStream.Read(array2, 0, array2.Length);
		}
		return Encoding.UTF8.GetString(array2);
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
