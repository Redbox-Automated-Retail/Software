using System;
using System.IO;
using ICSharpCode.SharpZipLib.GZip;

namespace Redbox.Compression;

public class GZipCompressionAlgorithm : CompressionAlgorithm
{
	private const int BufferSize = 1024000;

	public override byte[] Compress(byte[] source)
	{
		byte[] bytes;
		using (MemoryStream input = new MemoryStream(source))
		{
			BinaryReader binaryReader = new BinaryReader(input);
			using (MemoryStream memoryStream = new MemoryStream())
			{
				byte[] buffer = new byte[BufferSize];
				using GZipOutputStream gZipOutputStream = new GZipOutputStream(memoryStream);
				while (true)
				{
					int bytesRead = binaryReader.Read(buffer, 0, buffer.Length);
					if (bytesRead == 0)
					{
						break;
					}
					gZipOutputStream.Write(buffer, 0, bytesRead);
				}
				gZipOutputStream.Finish();
				bytes = new byte[memoryStream.Length];
				Array.Copy(memoryStream.GetBuffer(), 0L, bytes, 0L, memoryStream.Length);
			}
			binaryReader.Close();
		}
		return bytes;
	}

	public override byte[] Decompress(byte[] source)
	{
		byte[] bytes;
		using (MemoryStream memoryStream = new MemoryStream())
		{
			BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
			using (MemoryStream baseInputStream = new MemoryStream(source))
			{
				byte[] buffer = new byte[BufferSize];
				using GZipInputStream gZipInputStream = new GZipInputStream(baseInputStream, buffer.Length);
				while (true)
				{
					int bytesRead = gZipInputStream.Read(buffer, 0, buffer.Length);
					if (bytesRead == 0)
					{
						break;
					}
					binaryWriter.Write(buffer, 0, bytesRead);
				}
			}
			bytes = new byte[memoryStream.Length];
			Array.Copy(memoryStream.GetBuffer(), 0L, bytes, 0L, memoryStream.Length);
			binaryWriter.Close();
		}
		return bytes;
	}

	public override void Compress(Stream source, Stream target)
	{
		using BinaryReader binaryReader = new BinaryReader(source);
		byte[] buffer = new byte[BufferSize];
		using GZipOutputStream gZipOutputStream = new GZipOutputStream(target);
		while (true)
		{
			int bytesRead = binaryReader.Read(buffer, 0, buffer.Length);
			if (bytesRead == 0)
			{
				break;
			}
			gZipOutputStream.Write(buffer, 0, bytesRead);
		}
		gZipOutputStream.Finish();
	}

	public override void Decompress(Stream source, Stream target)
	{
		byte[] buffer = new byte[BufferSize];
		using GZipInputStream gZipInputStream = new GZipInputStream(source, buffer.Length);
		while (true)
		{
			int bytesRead = gZipInputStream.Read(buffer, 0, buffer.Length);
			if (bytesRead == 0)
			{
				break;
			}
			target.Write(buffer, 0, bytesRead);
		}
	}
}
