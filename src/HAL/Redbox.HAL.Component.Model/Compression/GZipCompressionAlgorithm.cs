using System;
using System.IO;
using ICSharpCode.SharpZipLib.GZip;

namespace Redbox.HAL.Component.Model.Compression;

internal sealed class GZipCompressionAlgorithm : ICompressionAlgorithm
{
	public byte[] Compress(byte[] source)
	{
		using MemoryStream input = StreamBase.NewOn(source);
		BinaryReader binaryReader = new BinaryReader(input);
		byte[] result;
		using (MemoryStream output = StreamBase.New())
		{
			GZipOutputStream gZipOutputStream = new GZipOutputStream(output);
			gZipOutputStream.Write(binaryReader.ReadBytes((int)input.Length), 0, (int)input.Length);
			gZipOutputStream.Finish();
			gZipOutputStream.Flush();
			result = new byte[output.Length];
			Array.Copy(output.GetBuffer()!, 0L, result, 0L, output.Length);
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
			GZipInputStream gZipInputStream = new GZipInputStream(input, 4096);
			while (true)
			{
				int bytesRead = gZipInputStream.Read(buffer, 0, 4096);
				if (bytesRead == 0)
				{
					break;
				}
				binaryWriter.Write(buffer, 0, bytesRead);
			}
		}
		byte[] result = new byte[output.Length];
		Array.Copy(output.GetBuffer()!, 0L, result, 0L, output.Length);
		binaryWriter.Close();
		return result;
	}
}
