using System;
using System.IO;
using ICSharpCode.SharpZipLib.GZip;

namespace Redbox.Compression;

public class GZipCompressionAlgorithm : CompressionAlgorithm
{
	private const int BufferSize = 1024000;

	public override byte[] Compress(byte[] source)
	{
		using MemoryStream input = new MemoryStream(source);
		BinaryReader binaryReader = new BinaryReader(input);
		byte[] array2;
		using (MemoryStream memoryStream = new MemoryStream())
		{
			byte[] array = new byte[1024000];
			using GZipOutputStream gZipOutputStream = new GZipOutputStream(memoryStream);
			while (true)
			{
				int num = binaryReader.Read(array, 0, array.Length);
				if (num == 0)
				{
					break;
				}
				gZipOutputStream.Write(array, 0, num);
			}
			gZipOutputStream.Finish();
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
			using GZipInputStream gZipInputStream = new GZipInputStream(baseInputStream, array.Length);
			while (true)
			{
				int num = gZipInputStream.Read(array, 0, array.Length);
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
		byte[] array = new byte[1024000];
		using GZipOutputStream gZipOutputStream = new GZipOutputStream(target);
		while (true)
		{
			int num = binaryReader.Read(array, 0, array.Length);
			if (num == 0)
			{
				break;
			}
			gZipOutputStream.Write(array, 0, num);
		}
		gZipOutputStream.Finish();
	}

	public override void Decompress(Stream source, Stream target)
	{
		byte[] array = new byte[1024000];
		using GZipInputStream gZipInputStream = new GZipInputStream(source, array.Length);
		while (true)
		{
			int num = gZipInputStream.Read(array, 0, array.Length);
			if (num == 0)
			{
				break;
			}
			target.Write(array, 0, num);
		}
	}
}
