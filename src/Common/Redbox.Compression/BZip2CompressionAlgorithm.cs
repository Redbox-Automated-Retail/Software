using System;
using System.IO;
using ICSharpCode.SharpZipLib.BZip2;

namespace Redbox.Compression;

public class BZip2CompressionAlgorithm : CompressionAlgorithm
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
			using BZip2OutputStream bZip2OutputStream = new BZip2OutputStream(memoryStream);
			while (true)
			{
				int num = binaryReader.Read(array, 0, array.Length);
				if (num == 0)
				{
					break;
				}
				bZip2OutputStream.Write(array, 0, num);
			}
			bZip2OutputStream.Flush();
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
		using (MemoryStream stream = new MemoryStream(source))
		{
			byte[] array = new byte[1024000];
			using BZip2InputStream bZip2InputStream = new BZip2InputStream(stream);
			while (true)
			{
				int num = bZip2InputStream.Read(array, 0, array.Length);
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
		byte[] array = new byte[1024000];
		using BinaryReader binaryReader = new BinaryReader(source);
		using BZip2OutputStream bZip2OutputStream = new BZip2OutputStream(target);
		while (true)
		{
			int num = binaryReader.Read(array, 0, array.Length);
			if (num == 0)
			{
				break;
			}
			bZip2OutputStream.Write(array, 0, num);
		}
		bZip2OutputStream.Flush();
	}

	public override void Decompress(Stream source, Stream target)
	{
		byte[] array = new byte[1024000];
		using BZip2InputStream bZip2InputStream = new BZip2InputStream(source);
		while (true)
		{
			int num = bZip2InputStream.Read(array, 0, array.Length);
			if (num == 0)
			{
				break;
			}
			target.Write(array, 0, num);
		}
	}
}
