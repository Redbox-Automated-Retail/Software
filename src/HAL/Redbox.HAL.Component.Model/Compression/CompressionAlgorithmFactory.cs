namespace Redbox.HAL.Component.Model.Compression;

public static class CompressionAlgorithmFactory
{
	public static ICompressionAlgorithm GetAlgorithm(CompressionType type)
	{
		return type switch
		{
			CompressionType.GZip => new GZipCompressionAlgorithm(), 
			CompressionType.LZMA => new LzmaCompressionAlgorithm(), 
			CompressionType.BZip2 => new BZip2CompressionAlgorithm(), 
			CompressionType.Zip => new ZipCompressionAlgorithm(), 
			_ => new NullCompressionAlgorithm(), 
		};
	}
}
