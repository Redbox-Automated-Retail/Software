namespace Redbox.HAL.Component.Model.Compression;

internal sealed class NullCompressionAlgorithm : ICompressionAlgorithm
{
	public byte[] Compress(byte[] source)
	{
		return source;
	}

	public byte[] Decompress(byte[] source)
	{
		return source;
	}
}
