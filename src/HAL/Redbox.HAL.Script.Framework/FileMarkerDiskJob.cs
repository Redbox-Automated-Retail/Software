using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "file-marker-disk", Operand = "FILE-MARKER-DISK")]
internal sealed class FileMarkerDiskJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		int deck = base.Context.PopTop<int>();
		int slot = base.Context.PopTop<int>();
		ErrorCodes errorCodes = base.MotionService.MoveVend(MoveMode.Get, base.Context.AppLog);
		if (errorCodes != ErrorCodes.Success)
		{
			HandleHWError(errorCodes.ToString());
			return;
		}
		IControllerService controllerService = ServiceLocator.Instance.GetService<IControllerService>();
		ErrorCodes acceptResult = controllerService.AcceptDiskAtDoor();
		switch (acceptResult)
		{
		case ErrorCodes.PickerEmpty:
			base.Context.CreateInfoResult("NoReceivedItem", "No disk was placed in the picker.");
			break;
		default:
			HandleHWError(acceptResult.ToString());
			break;
		case ErrorCodes.PickerFull:
		{
			errorCodes = ServiceLocator.Instance.GetService<IMotionControlService>().MoveTo(deck, slot, MoveMode.Put, base.Context.AppLog);
			if (errorCodes != ErrorCodes.Success)
			{
				HandleHWError(errorCodes.ToString());
				break;
			}
			base.ControlSystem.TrackCycle();
			if (controllerService.Put("UNKNOWN").Success)
			{
				base.Context.CreateResult("ItemReturned", "The marker disk was filed.", deck, slot, "UNKNOWN", null, null);
				break;
			}
			errorCodes = base.MotionService.MoveVend(MoveMode.None, base.Context.AppLog);
			if (errorCodes != ErrorCodes.Success)
			{
				HandleHWError(errorCodes.ToString());
				break;
			}
			IVendItemResult vendItemResult = controllerService.VendItemInPicker();
			if (vendItemResult.Status == ErrorCodes.PickerEmpty)
			{
				base.Context.CreateInfoResult("ItemTakenByMerchandizer", "The disk was taken by the merchandizer.");
				AddError("Disk not filed.");
			}
			else if (vendItemResult.Status == ErrorCodes.PickerFull)
			{
				base.Context.CreateInfoResult("ItemNotTaken", "The disk was taken by the merchandizer.");
				AddError("Disk not filed.");
			}
			else
			{
				HandleHWError(vendItemResult.Status.ToString());
			}
			break;
		}
		}
	}

	internal FileMarkerDiskJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}

	private void HandleHWError(string detail)
	{
		base.Context.Send("MachineError");
		base.Context.CreateInfoResult("MachineError", detail);
		AddError("Hardware Error during job.");
	}
}
