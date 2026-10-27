namespace Redbox.HAL.DataMatrix.Framework.Cortex;

internal sealed class IncompletePacket : CortexPacket
{
	internal override PacketType PacketType => PacketType.Incomplete;
}
