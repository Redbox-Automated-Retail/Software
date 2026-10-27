using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "put-disk-in-picker-in-bin")]
internal sealed class PutDiskInDumpBinJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		IDumpbinService service = ServiceLocator.Instance.GetService<IDumpbinService>();
		ApplicationLog applicationLog = ApplicationLog.ConfigureLog(base.Context, logAppends: true, "VMZ", flushOnWrite: true, "Tester-Put-In-Bin");
		IPickerSensorReadResult pickerSensorReadResult = base.ControlSystem.ReadPickerSensors();
		if (!pickerSensorReadResult.Success)
		{
			base.Context.CreateInfoResult("PickerSensorReadError", $"Read picker sensors failed with error {pickerSensorReadResult.Error}");
			AddError("Hardware error.");
			return;
		}
		if (!pickerSensorReadResult.IsFull)
		{
			base.Context.CreateInfoResult("PickerEmpty", "The picker is empty.");
			applicationLog.Write("The picker is empty.");
			AddError("The picker is empty.");
			return;
		}
		if (!ControllerConfiguration.Instance.IsVMZMachine)
		{
			base.Context.CreateInfoResult("Misconfiguration", "The dump bin is not configured.");
			applicationLog.Write("The dumpbin is not configured.");
			AddError("The bin is not configured.");
			return;
		}
		if (service.IsFull())
		{
			base.Context.CreateInfoResult("DumpBinFull", "The dump bin is full.");
			AddError("The dumpbin is full.");
			return;
		}
		ErrorCodes errorCodes = ServiceLocator.Instance.GetService<IMotionControlService>().MoveTo(service.PutLocation, MoveMode.Put, applicationLog);
		if (errorCodes != ErrorCodes.Success)
		{
			base.Context.CreateInfoResult("MoveError", $"MOVE failed with error {errorCodes.ToString()}");
			AddError("The MOVE instruction failed.");
			return;
		}
		string text = Scanner.ReadDiskInPicker(ReadDiskOptions.LeaveCaptureResult, base.Context);
		IPutResult putResult = ServiceLocator.Instance.GetService<IControllerService>().Put(text);
		if (!putResult.Success)
		{
			base.Context.CreateInfoResult("PutError", $"PUT failed with error {putResult.ToString()}");
			AddError("The PUT instruction failed.");
			return;
		}
		base.Context.CreateInfoResult("PutToBinOK", "The disk was successfully filed", text);
		applicationLog.WriteFormatted("The barcode {0} was put into the dump bin.", text);
		service.DumpContents(base.AppLog);
	}

	internal PutDiskInDumpBinJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
