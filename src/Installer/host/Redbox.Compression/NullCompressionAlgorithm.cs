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
		byte[] buffer = new byte[4096];
		for (int bytesRead = source.Read(buffer, 0, buffer.Length); bytesRead > 0; bytesRead = source.Read(buffer, 0, buffer.Length))
		{
			target.Write(buffer, 0, bytesRead);
		}
	}

	public override void Decompress(Stream source, Stream target)
	{
		byte[] buffer = new byte[4096];
		for (int bytesRead = source.Read(buffer, 0, buffer.Length); bytesRead > 0; bytesRead = source.Read(buffer, 0, buffer.Length))
		{
			target.Write(buffer, 0, bytesRead);
		}
	}
}
