using System.IO;

namespace Redbox.Compression;

public class NullCompressionAlgorithm : CompressionAlgorithm
{
	public override byte[] Compress(byte[] source)
	{
		return source;
	}

	public override byte[] Decompress(byte[] source)
	{
		return source;
	}

	public override void Compress(Stream source, Stream target)
	{
		byte[] array = new byte[4096];
		for (int num = source.Read(array, 0, array.Length); num > 0; num = source.Read(array, 0, array.Length))
		{
			target.Write(array, 0, num);
		}
	}

	public override void Decompress(Stream source, Stream target)
	{
		byte[] array = new byte[4096];
		for (int num = source.Read(array, 0, array.Length); num > 0; num = source.Read(array, 0, array.Length))
		{
			target.Write(array, 0, num);
		}
	}
}
