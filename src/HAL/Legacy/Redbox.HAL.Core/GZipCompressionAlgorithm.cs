using System;
using System.IO;
using ICSharpCode.SharpZipLib.GZip;

namespace Redbox.HAL.Core;

public class GZipCompressionAlgorithm : CompressionAlgorithm
{
	public override byte[] Compress(byte[] source)
	{
		using MemoryStream memoryStream = StreamBase.NewOn(source);
		BinaryReader binaryReader = new BinaryReader(memoryStream);
		byte[] array;
		using (MemoryStream memoryStream2 = StreamBase.New())
		{
			GZipOutputStream gZipOutputStream = new GZipOutputStream(memoryStream2);
			gZipOutputStream.Write(binaryReader.ReadBytes((int)memoryStream.Length), 0, (int)memoryStream.Length);
			gZipOutputStream.Finish();
			gZipOutputStream.Flush();
			array = new byte[memoryStream2.Length];
			Array.Copy(memoryStream2.GetBuffer(), 0L, array, 0L, memoryStream2.Length);
		}
		binaryReader.Close();
		return array;
	}

	public override byte[] Decompress(byte[] source)
	{
		using MemoryStream memoryStream = StreamBase.New();
		BinaryWriter binaryWriter = new BinaryWriter(memoryStream);
		using (MemoryStream memoryStream2 = StreamBase.NewOn(source))
		{
			byte[] buffer = new byte[4096];
			GZipInputStream gZipInputStream = new GZipInputStream(memoryStream2, 4096);
			while (true)
			{
				int num = gZipInputStream.Read(buffer, 0, 4096);
				if (num != 0)
				{
					binaryWriter.Write(buffer, 0, num);
					continue;
				}
				break;
			}
		}
		byte[] array = new byte[memoryStream.Length];
		Array.Copy(memoryStream.GetBuffer(), 0L, array, 0L, memoryStream.Length);
		binaryWriter.Close();
		return array;
	}
}
