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
		int num = base.Context.PopTop<int>();
		int slot = base.Context.PopTop<int>();
		if (!MoveAndGet(num, slot))
		{
			AddError("Failure to retrieve disk from start position.");
			return;
		}
		int num2 = base.Context.PopTop<int>();
		bool flag = false;
		int[] array = new int[2] { 30, 60 };
		int[] array2 = new int[3] { 1, 30, 60 };
		for (int i = 1; i <= num2; i++)
		{
			if (flag)
			{
				break;
			}
			int[] array3 = ((i == num) ? array : array2);
			foreach (int slot2 in array3)
			{
				if (!MoveAndPut(i, slot2, "UNKNOWN"))
				{
					AddError("Unable to place the disk at a test location.");
					flag = true;
					break;
				}
				if (!MoveAndGet(i, slot2))
				{
					AddError("Unable to get the disk from test location.");
					flag = true;
					break;
				}
			}
		}
		if (!flag)
		{
			if (!MoveAndPut(num, slot, "UNKNOWN"))
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
