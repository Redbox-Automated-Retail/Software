using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework.Natives;

[NativeJob(ProgramName = "one-disk-quick-deck-test-kent")]
internal sealed class OneDiskQuickTestKentJob : NativeJobAdapter
{
	private readonly int[] TestSlots = new int[2] { 30, 60 };

	protected override void ExecuteInner()
	{
		ApplicationLog appLog = ApplicationLog.ConfigureLog(base.Context, logAppends: true, "KioskTest", flushOnWrite: false, "OneDiskQuickTest");
		int errors = 0;
		IDecksService decksService = ServiceLocator.Instance.GetService<IDecksService>();
		decksService.ForAllDecksDo(delegate(IDeck deck)
		{
			if (deck.IsQlm)
			{
				appLog.Write("The deck is a qlm deck - finish test.");
				return true;
			}
			if (!TestDeck(deck))
			{
				int num = errors + 1;
				errors = num;
				return false;
			}
			IDeck byNumber = decksService.GetByNumber(deck.Number + 1);
			if (byNumber == null || byNumber.IsQlm)
			{
				return true;
			}
			if (!MoveAndPut(byNumber.Number, 1, "UNKNOWN"))
			{
				AddError("Unable to place the into the slot.");
				int num = errors + 1;
				errors = num;
				return false;
			}
			return true;
		});
		if (errors <= 0)
		{
			if (!MoveAndPut(1, 1, "UNKNOWN"))
			{
				AddError("Unable to place the disk back into the source slot.");
			}
			ServiceLocator.Instance.GetService<IMotionControlService>().MoveVend(MoveMode.Get, appLog);
		}
	}

	internal OneDiskQuickTestKentJob(ExecutionResult r, ExecutionContext c)
		: base(r, c)
	{
	}

	private bool TestDeck(IDeck deck)
	{
		if (!MoveAndGet(deck.Number, 1))
		{
			AddError("Failure to retrieve disk from start position.");
			return false;
		}
		int[] testSlots = TestSlots;
		foreach (int slot in testSlots)
		{
			if (!MoveAndPut(deck.Number, slot, "UNKNOWN"))
			{
				AddError("Unable to place the disk at a test location.");
				return false;
			}
			if (!MoveAndGet(deck.Number, slot))
			{
				AddError("Unable to get the disk from test location.");
				return false;
			}
		}
		return true;
	}
}
