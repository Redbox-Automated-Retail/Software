namespace Redbox.HAL.Client.Executors;

public sealed class PutInLocationResult : JobExecutor
{
	private readonly int Deck;

	private readonly int Slot;

	protected override string JobName => "put-disk-to-location";

	public PutInLocationResult(HardwareService service, int deck, int slot)
		: base(service)
	{
		Deck = deck;
		Slot = slot;
	}

	protected override void SetupJob()
	{
		base.Job.Push(Deck, Slot);
	}
}
