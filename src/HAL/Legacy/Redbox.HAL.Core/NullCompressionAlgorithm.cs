namespace Redbox.HAL.Core;

public sealed class NullCompressionAlgorithm : CompressionAlgorithm
{
	public override byte[] Compress(byte[] source)
	{
		return source;
	}

	public override byte[] Decompress(byte[] source)
	{
		return source;
	}
}
