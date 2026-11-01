using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "one-disk-quick-deck-test")]
internal sealed class OneDiskQuickTestJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		ApplicationLog applicationLog = ApplicationLog.ConfigureLog(base.Context, logAppends: true, "KioskTest", flushOnWrite: false, "OneDiskQuickTest");
		if (!ControllerConfiguration.Instance.IsVMZMachine)
		{
			applicationLog.WriteFormatted("** the machine is not configured for a VMZ - bailing **");
			AddError("Misconfigured test");
			return;
		}
		int startDeck = base.Context.PopTop<int>();
		int startSlot = base.Context.PopTop<int>();
		if (!MoveAndGet(startDeck, startSlot))
		{
			AddError("Failure to retrieve disk from start position.");
			return;
		}
		int deckCount = base.Context.PopTop<int>();
		bool failed = false;
		int[] startDeckSlots = new int[2] { 30, 60 };
		int[] testSlots = new int[3] { 1, 30, 60 };
		for (int i = 1; i <= deckCount; i++)
		{
			if (failed)
			{
				break;
			}
			foreach (int testSlot in ((i == startDeck) ? startDeckSlots : testSlots))
			{
				if (!MoveAndPut(i, testSlot, "UNKNOWN"))
				{
					AddError("Unable to place the disk at a test location.");
					failed = true;
					break;
				}
				if (!MoveAndGet(i, testSlot))
				{
					AddError("Unable to get the disk from test location.");
					failed = true;
					break;
				}
			}
		}
		if (!failed)
		{
			if (!MoveAndPut(startDeck, startSlot, "UNKNOWN"))
			{
				AddError("Unable to place the disk at a test location.");
			}
			else
			{
				base.MotionService.MoveVend(MoveMode.Get, applicationLog);
			}
		}
	}

	internal OneDiskQuickTestJob(ExecutionResult r, ExecutionContext c)
		: base(r, c)
	{
	}
}
