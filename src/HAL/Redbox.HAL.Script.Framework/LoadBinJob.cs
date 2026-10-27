using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "load-bin", Operand = "LOAD-BIN")]
internal sealed class LoadBinJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		ApplicationLog applicationLog = ApplicationLog.ConfigureLog(base.Context, logAppends: false, "KioskTest", flushOnWrite: false, "load-bin-fast");
		if (!ControllerConfiguration.Instance.IsVMZMachine)
		{
			applicationLog.Write("The dump bin is not configured.");
			AddError("The dump bin is not configured.");
			return;
		}
		string text = base.Context.PopTop<string>();
		LoadBy loadBy = Enum<LoadBy>.ParseIgnoringCase(text, LoadBy.None);
		if (loadBy == LoadBy.None)
		{
			string text2 = $"Unrecognized argument {text}";
			applicationLog.Write(text2);
			AddError(text2);
			return;
		}
		int num = base.Context.PopTop<int>();
		for (int i = 0; i < num; i++)
		{
			if (base.DumpbinService.IsFull())
			{
				return;
			}
			ILocation location = null;
			if (LoadBy.Barcode == loadBy)
			{
				string id = base.Context.PopTop<string>();
				location = base.InventoryService.Lookup(id);
				if (location == null)
				{
					continue;
				}
			}
			else
			{
				int deck = base.Context.PopTop<int>();
				int slot = base.Context.PopTop<int>();
				location = base.InventoryService.Get(deck, slot);
			}
			if (base.MotionService.MoveTo(location, MoveMode.Get, base.AppLog) != ErrorCodes.Success)
			{
				AddError("There was an error during the move.");
				return;
			}
			IControllerService service = ServiceLocator.Instance.GetService<IControllerService>();
			IGetResult getResult = service.Get();
			if (!getResult.Success)
			{
				AddError("Unable to fetch disk from drum.");
				return;
			}
			if (base.MotionService.MoveTo(base.DumpbinService.PutLocation, MoveMode.Put, base.AppLog) != ErrorCodes.Success)
			{
				AddError("There was an error during the move.");
				return;
			}
			if (!service.Put(getResult.Previous).Success)
			{
				AddError("Unable to place disk into drum.");
				return;
			}
		}
		if (base.MotionService.MoveVend(MoveMode.Get, base.AppLog) != ErrorCodes.Success)
		{
			AddError("Unable to move to vend.");
		}
	}

	internal LoadBinJob(ExecutionResult r, ExecutionContext c)
		: base(r, c)
	{
	}
}
