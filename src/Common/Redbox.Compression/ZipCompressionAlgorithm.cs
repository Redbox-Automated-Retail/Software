using System;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;

namespace Redbox.Compression;

public class ZipCompressionAlgorithm : CompressionAlgorithm
{
	private const int BufferSize = 1024000;

	public override byte[] Compress(byte[] source)
	{
		byte[] array = new byte[1024000];
		using MemoryStream input = new MemoryStream(source);
		BinaryReader binaryReader = new BinaryReader(input);
		byte[] array2;
		using (MemoryStream memoryStream = new MemoryStream())
		{
			ZipOutputStream zipOutputStream = new ZipOutputStream(memoryStream);
			ZipEntry entry = new ZipEntry("file")
			{
				DateTime = DateTime.UtcNow,
				Size = source.Length,
				CompressionMethod = CompressionMethod.Deflated
			};
			zipOutputStream.PutNextEntry(entry);
			while (true)
			{
				int num = binaryReader.Read(array, 0, array.Length);
				if (num == 0)
				{
					break;
				}
				zipOutputStream.Write(array, 0, num);
			}
			zipOutputStream.Finish();
			zipOutputStream.Flush();
			array2 = new byte[memoryStream.Length];
			Array.Copy(memoryStream.GetBuffer(), 0L, array2, 0L, memoryStream.Length);
		}
		binaryReader.Close();
		return array2;
	}

	public override byte[] Decompress(byte[] source)
	{
		using MemoryStream memoryStream = new MemoryStream();
		BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		using (MemoryStream baseInputStream = new MemoryStream(source))
		{
			byte[] array = new byte[1024000];
			ZipInputStream zipInputStream = new ZipInputStream(baseInputStream);
			while (true)
			{
				int num = zipInputStream.Read(array, 0, array.Length);
				if (num == 0)
				{
					break;
				}
				binaryWriter.Write(array, 0, num);
			}
		}
		byte[] array2 = new byte[memoryStream.Length];
		Array.Copy(memoryStream.GetBuffer(), 0L, array2, 0L, memoryStream.Length);
		binaryWriter.Close();
		return array2;
	}

	public override void Compress(Stream source, Stream target)
	{
		using BinaryReader binaryReader = new BinaryReader(source);
		using ZipOutputStream zipOutputStream = new ZipOutputStream(target);
		ZipEntry entry = new ZipEntry("file")
		{
			DateTime = DateTime.UtcNow,
			Size = source.Length,
			CompressionMethod = CompressionMethod.Deflated
		};
		zipOutputStream.PutNextEntry(entry);
		zipOutputStream.Write(binaryReader.ReadBytes((int)source.Length), 0, (int)source.Length);
		zipOutputStream.Finish();
	}

	public override void Decompress(Stream source, Stream target)
	{
		byte[] array = new byte[1024000];
		using ZipInputStream zipInputStream = new ZipInputStream(source);
		while (true)
		{
			int num = zipInputStream.Read(array, 0, array.Length);
			if (num == 0)
			{
				break;
			}
			target.Write(array, 0, num);
		}
	}
}
