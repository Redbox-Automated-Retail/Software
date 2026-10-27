using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework.Natives;

[NativeJob(ProgramName = "put-disk-to-location")]
internal sealed class PutDiskToLocationJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		int num = base.Context.PopTop<int>();
		int num2 = base.Context.PopTop<int>();
		ErrorCodes errorCodes = base.MotionService.MoveTo(num2, num, MoveMode.Put, base.AppLog);
		if (errorCodes != ErrorCodes.Success)
		{
			base.Context.CreateInfoResult("JobError", $"MOVE DECK={num2} SLOT={num} MODE=PUT {errorCodes.ToString().ToUpper()}");
			return;
		}
		string text = "UNKNOWN";
		using (Scanner scanner = new Scanner(CenterDiskMethod.VendDoorAndBack, base.ControlSystem))
		{
			using ScanResult scanResult = scanner.Read();
			text = scanResult.ScannedMatrix;
		}
		IPutResult putResult = ServiceLocator.Instance.GetService<IControllerService>().Put(text);
		if (putResult.Success)
		{
			base.Context.CreateInfoResult("JobSuccess", $"PUT SUCCESS (barcode = {text})");
		}
		else
		{
			base.Context.CreateInfoResult("JobError", $"PUT {putResult.ToString()}");
		}
	}

	internal PutDiskToLocationJob(ExecutionResult r, ExecutionContext c)
		: base(r, c)
	{
	}
}
