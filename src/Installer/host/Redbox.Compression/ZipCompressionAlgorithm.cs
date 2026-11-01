using System;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;

namespace Redbox.Compression;

public class ZipCompressionAlgorithm : CompressionAlgorithm
{
	private const int BufferSize = 1024000;

	public override byte[] Compress(byte[] source)
	{
		byte[] buffer = new byte[BufferSize];
		byte[] bytes;
		using (MemoryStream input = new MemoryStream(source))
		{
			BinaryReader binaryReader = new BinaryReader(input);
			using (MemoryStream memoryStream = new MemoryStream())
			{
				ZipOutputStream zipOutputStream = new ZipOutputStream(memoryStream);
				ZipEntry zipEntry = new ZipEntry("file");
				zipEntry.DateTime = DateTime.UtcNow;
				zipEntry.Size = source.Length;
				zipEntry.CompressionMethod = CompressionMethod.Deflated;
				ZipEntry entry = zipEntry;
				zipOutputStream.PutNextEntry(entry);
				while (true)
				{
					bool flag = true;
					int bytesRead = binaryReader.Read(buffer, 0, buffer.Length);
					if (bytesRead == 0)
					{
						break;
					}
					zipOutputStream.Write(buffer, 0, bytesRead);
				}
				zipOutputStream.Finish();
				zipOutputStream.Flush();
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
				ZipInputStream zipInputStream = new ZipInputStream(baseInputStream);
				while (true)
				{
					bool flag = true;
					int bytesRead = zipInputStream.Read(buffer, 0, buffer.Length);
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
		using ZipOutputStream zipOutputStream = new ZipOutputStream(target);
		ZipEntry zipEntry = new ZipEntry("file");
		zipEntry.DateTime = DateTime.UtcNow;
		zipEntry.Size = source.Length;
		zipEntry.CompressionMethod = CompressionMethod.Deflated;
		ZipEntry entry = zipEntry;
		zipOutputStream.PutNextEntry(entry);
		zipOutputStream.Write(binaryReader.ReadBytes((int)source.Length), 0, (int)source.Length);
		zipOutputStream.Finish();
	}

	public override void Decompress(Stream source, Stream target)
	{
		byte[] buffer = new byte[BufferSize];
		using ZipInputStream zipInputStream = new ZipInputStream(source);
		while (true)
		{
			bool flag = true;
			int bytesRead = zipInputStream.Read(buffer, 0, buffer.Length);
			if (bytesRead == 0)
			{
				break;
			}
			target.Write(buffer, 0, bytesRead);
		}
	}
}
