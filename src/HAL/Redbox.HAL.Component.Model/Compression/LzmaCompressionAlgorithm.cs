using SevenZip.Compression.LZMA;

namespace Redbox.HAL.Component.Model.Compression;

internal sealed class LzmaCompressionAlgorithm : ICompressionAlgorithm
{
	public byte[] Compress(byte[] source)
	{
		return SevenZipHelper.Compress(source);
	}

	public byte[] Decompress(byte[] source)
	{
		return SevenZipHelper.Decompress(source);
	}
}
