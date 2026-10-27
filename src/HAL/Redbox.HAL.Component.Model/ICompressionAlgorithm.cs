namespace Redbox.HAL.Component.Model;

public interface ICompressionAlgorithm
{
	byte[] Compress(byte[] source);

	byte[] Decompress(byte[] source);
}
