namespace Redbox.HAL.Client.Executors;

public sealed class OneDiskQuickTest : JobExecutor
{
	private readonly int SourceDeck;

	private readonly int SourceSlot;

	private readonly int DeckCount;

	protected override string JobName => "one-disk-quick-deck-test";

	public OneDiskQuickTest(HardwareService service, int sourceDeck, int sourceSlot, int count)
		: base(service)
	{
		SourceDeck = sourceDeck;
		SourceSlot = sourceSlot;
		DeckCount = count;
	}

	protected override void SetupJob()
	{
		base.Job.Push(DeckCount);
		base.Job.Push(SourceSlot);
		base.Job.Push(SourceDeck);
	}
}
