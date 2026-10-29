namespace Redbox.Compression;

public static class ByteArrayExtensions
{
	public static byte[] Compress(this byte[] buffer, CompressionType type)
	{
		return CompressionAlgorithm.GetAlgorithm(type).Compress(buffer);
	}

	public static byte[] Decompress(this byte[] buffer, CompressionType type)
	{
		return CompressionAlgorithm.GetAlgorithm(type).Decompress(buffer);
	}
}
