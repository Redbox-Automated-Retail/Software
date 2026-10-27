using Redbox.HAL.Component.Model;

namespace Redbox.HAL.IPC.Framework;

public static class CompressionAlgorithm
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
