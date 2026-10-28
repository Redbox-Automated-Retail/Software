using System;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;

namespace Redbox.HAL.Core;

public class ZipCompressionAlgorithm : CompressionAlgorithm
{
	public override byte[] Compress(byte[] source)
	{
		using MemoryStream memoryStream = StreamBase.NewOn(source);
		BinaryReader binaryReader = new BinaryReader(memoryStream);
		byte[] array;
		using (MemoryStream memoryStream2 = StreamBase.New())
		{
			ZipOutputStream zipOutputStream = new ZipOutputStream(memoryStream2);
			zipOutputStream.Write(binaryReader.ReadBytes((int)memoryStream.Length), 0, (int)memoryStream.Length);
			zipOutputStream.Finish();
			zipOutputStream.Flush();
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
			ZipInputStream zipInputStream = new ZipInputStream(memoryStream2);
			while (true)
			{
				int num = zipInputStream.Read(buffer, 0, 4096);
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
