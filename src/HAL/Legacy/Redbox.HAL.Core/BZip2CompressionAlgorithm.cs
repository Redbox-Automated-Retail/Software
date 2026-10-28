using System;
using System.IO;
using ICSharpCode.SharpZipLib.BZip2;

namespace Redbox.HAL.Core;

public class BZip2CompressionAlgorithm : CompressionAlgorithm
{
	public override byte[] Compress(byte[] source)
	{
		using MemoryStream memoryStream = StreamBase.NewOn(source);
		BinaryReader binaryReader = new BinaryReader(memoryStream);
		byte[] array;
		using (MemoryStream memoryStream2 = StreamBase.New())
		{
			BZip2OutputStream bZip2OutputStream = new BZip2OutputStream(memoryStream2);
			bZip2OutputStream.Write(binaryReader.ReadBytes((int)memoryStream.Length), 0, (int)memoryStream.Length);
			bZip2OutputStream.Flush();
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
			BZip2InputStream bZip2InputStream = new BZip2InputStream(memoryStream2);
			while (true)
			{
				int num = bZip2InputStream.Read(buffer, 0, 4096);
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
