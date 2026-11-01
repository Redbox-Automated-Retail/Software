using System;
using System.IO;
using ICSharpCode.SharpZipLib.BZip2;

namespace Redbox.Compression;

public class BZip2CompressionAlgorithm : CompressionAlgorithm
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
				using BZip2OutputStream bZip2OutputStream = new BZip2OutputStream(memoryStream);
				while (true)
				{
					int bytesRead = binaryReader.Read(buffer, 0, buffer.Length);
					if (bytesRead == 0)
					{
						break;
					}
					bZip2OutputStream.Write(buffer, 0, bytesRead);
				}
				bZip2OutputStream.Flush();
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
			using (MemoryStream stream = new MemoryStream(source))
			{
				byte[] buffer = new byte[BufferSize];
				using BZip2InputStream bZip2InputStream = new BZip2InputStream(stream);
				while (true)
				{
					int bytesRead = bZip2InputStream.Read(buffer, 0, buffer.Length);
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
		byte[] buffer = new byte[BufferSize];
		using BinaryReader binaryReader = new BinaryReader(source);
		using BZip2OutputStream bZip2OutputStream = new BZip2OutputStream(target);
		while (true)
		{
			int bytesRead = binaryReader.Read(buffer, 0, buffer.Length);
			if (bytesRead == 0)
			{
				break;
			}
			bZip2OutputStream.Write(buffer, 0, bytesRead);
		}
		bZip2OutputStream.Flush();
	}

	public override void Decompress(Stream source, Stream target)
	{
		byte[] buffer = new byte[BufferSize];
		using BZip2InputStream bZip2InputStream = new BZip2InputStream(source);
		while (true)
		{
			int bytesRead = bZip2InputStream.Read(buffer, 0, buffer.Length);
			if (bytesRead == 0)
			{
				break;
			}
			target.Write(buffer, 0, bytesRead);
		}
	}
}
