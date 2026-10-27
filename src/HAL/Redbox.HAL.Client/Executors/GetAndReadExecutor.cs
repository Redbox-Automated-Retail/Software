namespace Redbox.HAL.Client.Executors;

public sealed class GetAndReadExecutor : JobExecutor
{
	private readonly int Deck;

	private readonly int Slot;

	private readonly bool Center;

	protected override string JobName => "get-and-read";

	public GetAndReadExecutor(HardwareService service, int deck, int slot, bool center)
		: base(service)
	{
		Deck = deck;
		Slot = slot;
		Center = center;
	}

	protected override void SetupJob()
	{
		base.Job.Push(Center.ToString(), Deck, Slot);
	}
}
