using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "test-retrofit-deck")]
internal sealed class VMZRetrofitTest : NativeJobAdapter
{
	private readonly int[] TestSlots = new int[5] { 16, 31, 46, 61, 76 };

	protected override void ExecuteInner()
	{
		try
		{
			ApplicationLog applicationLog = ApplicationLog.ConfigureLog(base.Context, logAppends: true, "KioskTest", flushOnWrite: false, "RetrofitDeckTest");
			if (!ControllerConfiguration.Instance.IsVMZMachine)
			{
				applicationLog.Write("The kiosk is still configured for a QLM - bail test.");
				AddError("Kiosk Misconfiguration.");
				return;
			}
			IDeck byNumber = base.DecksService.GetByNumber(base.DumpbinService.PutLocation.Deck);
			if (!MoveAndGet(byNumber.Number, 1))
			{
				AddError("Failure to retrieve disk from start position.");
				return;
			}
			int[] testSlots = TestSlots;
			foreach (int slot in testSlots)
			{
				if (!MoveAndPut(byNumber.Number, slot, "UNKNOWN"))
				{
					AddError("Unable to place the disk at a test location.");
					return;
				}
				if (!MoveAndGet(byNumber.Number, slot))
				{
					AddError("Unable to get the disk from test location.");
					return;
				}
			}
			if (!MoveAndPut(byNumber.Number, 81, "UNKNOWN"))
			{
				AddError("Unable to put disk into adjacent slot.");
			}
			else if (TestSlotForDisk(byNumber.Number, 81))
			{
				if (!PutToAdjacentAndTest(byNumber.Number, 2, 85))
				{
					AddError("Test failure!");
				}
				else if (!MoveAndGet(byNumber.Number, 3))
				{
					AddError("Test failure!");
				}
				else if (!TestAdjacentWithDisk(byNumber.Number, 81))
				{
					AddError("Test failure!");
				}
				else if (!TestAdjacentWithDisk(byNumber.Number, 85))
				{
					AddError("Test failure!");
				}
				else if (!FileToBin(byNumber.Number))
				{
					AddError("Test failure!");
				}
				else if (!MoveAndGet(byNumber.Number, 81))
				{
					AddError("Test failure!");
				}
				else if (!FileToBin(byNumber.Number))
				{
					AddError("Test failure!");
				}
				else if (!MoveAndGet(byNumber.Number, 85))
				{
					AddError("Test failure!");
				}
				else if (!FileToBin(byNumber.Number))
				{
					AddError("Test failure!");
				}
				else
				{
					base.MotionService.MoveVend(MoveMode.Get, base.AppLog);
				}
			}
		}
		finally
		{
			IDeck byNumber2 = base.DecksService.GetByNumber(base.DumpbinService.PutLocation.Deck);
			if (!byNumber2.IsQlm)
			{
				base.InventoryService.MarkDeckInventory(byNumber2, "EMPTY");
			}
			base.DumpbinService.ClearItems();
		}
	}

	internal VMZRetrofitTest(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}

	private bool FileToBin(int deck)
	{
		if (base.MotionService.MoveTo(base.DumpbinService.PutLocation, MoveMode.Put, base.AppLog) != ErrorCodes.Success)
		{
			AddError("Test failure!");
			return false;
		}
		if (!ServiceLocator.Instance.GetService<IControllerService>().Put("UNKNOWN").Success)
		{
			AddError("Test failure.");
			return false;
		}
		base.MotionService.MoveTo(deck, 1, MoveMode.None, base.AppLog);
		return true;
	}

	private bool TestAdjacentWithDisk(int deck, int testSlot)
	{
		if (base.MotionService.MoveTo(deck, testSlot, MoveMode.Put, base.AppLog) != ErrorCodes.Success)
		{
			AddError("Test failure!");
			return false;
		}
		base.ControlSystem.TrackCycle();
		if (!ServiceLocator.Instance.GetService<IControllerService>().Put("UNKNOWN").IsSlotInUse)
		{
			return false;
		}
		return TestSlotForDisk(deck, testSlot);
	}

	private bool PutToAdjacentAndTest(int deck, int slot, int targetSlot)
	{
		if (!MoveAndGet(deck, slot))
		{
			AddError("Unable to retrieve source disk.");
			return false;
		}
		if (!MoveAndPut(deck, targetSlot, "UNKNOWN"))
		{
			AddError("Unable to put disk into adjacent slot.");
			return false;
		}
		return TestSlotForDisk(deck, targetSlot);
	}

	private bool TestSlotForDisk(int deck, int slot)
	{
		return true;
	}
}
