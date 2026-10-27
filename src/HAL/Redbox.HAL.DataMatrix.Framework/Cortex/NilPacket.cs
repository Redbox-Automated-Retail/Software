namespace Redbox.HAL.DataMatrix.Framework.Cortex;

internal sealed class NilPacket : CortexPacket
{
	internal override PacketType PacketType => PacketType.Invalid;

	internal NilPacket(byte[] packet)
	{
	}
}
