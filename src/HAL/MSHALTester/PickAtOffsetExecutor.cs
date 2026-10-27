using Redbox.HAL.Client;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.MSHALTester;

internal sealed class PickAtOffsetExecutor : JobExecutor
{
	private readonly int Deck;

	private readonly int Slot;

	private readonly int Offset;

	private readonly Axis Axis;

	protected override string JobName => "pick-at-offset";

	protected override void SetupJob()
	{
		base.Job.Push(Offset);
		base.Job.Push(Axis.ToString());
		base.Job.Push(Slot);
		base.Job.Push(Deck);
	}

	internal PickAtOffsetExecutor(HardwareService service, int deck, int slot, Axis axis, int offset)
		: base(service)
	{
		Deck = deck;
		Slot = slot;
		Axis = axis;
		Offset = offset;
	}
}
