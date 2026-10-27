using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "file-marker-disk", Operand = "FILE-MARKER-DISK")]
internal sealed class FileMarkerDiskJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		int num = base.Context.PopTop<int>();
		int num2 = base.Context.PopTop<int>();
		ErrorCodes errorCodes = base.MotionService.MoveVend(MoveMode.Get, base.Context.AppLog);
		if (errorCodes != ErrorCodes.Success)
		{
			HandleHWError(errorCodes.ToString());
			return;
		}
		IControllerService service = ServiceLocator.Instance.GetService<IControllerService>();
		ErrorCodes errorCodes2 = service.AcceptDiskAtDoor();
		switch (errorCodes2)
		{
		case ErrorCodes.PickerEmpty:
			base.Context.CreateInfoResult("NoReceivedItem", "No disk was placed in the picker.");
			break;
		default:
			HandleHWError(errorCodes2.ToString());
			break;
		case ErrorCodes.PickerFull:
		{
			errorCodes = ServiceLocator.Instance.GetService<IMotionControlService>().MoveTo(num, num2, MoveMode.Put, base.Context.AppLog);
			if (errorCodes != ErrorCodes.Success)
			{
				HandleHWError(errorCodes.ToString());
				break;
			}
			base.ControlSystem.TrackCycle();
			if (service.Put("UNKNOWN").Success)
			{
				base.Context.CreateResult("ItemReturned", "The marker disk was filed.", num, num2, "UNKNOWN", null, null);
				break;
			}
			errorCodes = base.MotionService.MoveVend(MoveMode.None, base.Context.AppLog);
			if (errorCodes != ErrorCodes.Success)
			{
				HandleHWError(errorCodes.ToString());
				break;
			}
			IVendItemResult vendItemResult = service.VendItemInPicker();
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
