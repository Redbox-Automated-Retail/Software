using System.IO;

namespace Redbox.Compression;

public abstract class CompressionAlgorithm
{
	public static CompressionAlgorithm GetAlgorithm(CompressionType type)
	{
		return type switch
		{
			CompressionType.None => new NullCompressionAlgorithm(), 
			CompressionType.GZip => new GZipCompressionAlgorithm(), 
			CompressionType.LZMA => new LzmaCompressionAlgorithm(), 
			CompressionType.BZip2 => new BZip2CompressionAlgorithm(), 
			CompressionType.Zip => new ZipCompressionAlgorithm(), 
			_ => new NullCompressionAlgorithm(), 
		};
	}

	public abstract byte[] Compress(byte[] source);

	public abstract byte[] Decompress(byte[] source);

	public abstract void Compress(Stream source, Stream target);

	public abstract void Decompress(Stream source, Stream target);
}
