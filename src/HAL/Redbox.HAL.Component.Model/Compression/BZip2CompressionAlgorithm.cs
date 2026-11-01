using System;
using System.IO;
using ICSharpCode.SharpZipLib.BZip2;

namespace Redbox.HAL.Component.Model.Compression;

internal sealed class BZip2CompressionAlgorithm : ICompressionAlgorithm
{
	public byte[] Compress(byte[] source)
	{
		using MemoryStream input = StreamBase.NewOn(source);
		BinaryReader binaryReader = new BinaryReader(input);
		byte[] result;
		using (MemoryStream output = StreamBase.New())
		{
			BZip2OutputStream bZip2OutputStream = new BZip2OutputStream(output);
			bZip2OutputStream.Write(binaryReader.ReadBytes((int)input.Length), 0, (int)input.Length);
			bZip2OutputStream.Flush();
			result = new byte[output.Length];
			Array.Copy(output.GetBuffer(), 0L, result, 0L, output.Length);
		}
		binaryReader.Close();
		return result;
	}

	public byte[] Decompress(byte[] source)
	{
		using MemoryStream output = StreamBase.New();
		BinaryWriter binaryWriter = new BinaryWriter(output);
		using (MemoryStream input = StreamBase.NewOn(source))
		{
			byte[] buffer = new byte[4096];
			BZip2InputStream bZip2InputStream = new BZip2InputStream(input);
			while (true)
			{
				int bytesRead = bZip2InputStream.Read(buffer, 0, 4096);
				if (bytesRead == 0)
				{
					break;
				}
				binaryWriter.Write(buffer, 0, bytesRead);
			}
		}
		byte[] result = new byte[output.Length];
		Array.Copy(output.GetBuffer(), 0L, result, 0L, output.Length);
		binaryWriter.Close();
		return result;
	}
}
