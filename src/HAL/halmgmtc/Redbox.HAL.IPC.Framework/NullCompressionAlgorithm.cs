using Redbox.HAL.Component.Model;

namespace Redbox.HAL.IPC.Framework;

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
