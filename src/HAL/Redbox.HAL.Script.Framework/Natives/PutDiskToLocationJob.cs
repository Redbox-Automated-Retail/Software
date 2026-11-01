using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework.Natives;

[NativeJob(ProgramName = "put-disk-to-location")]
internal sealed class PutDiskToLocationJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		int slot = base.Context.PopTop<int>();
		int deck = base.Context.PopTop<int>();
		ErrorCodes errorCodes = base.MotionService.MoveTo(deck, slot, MoveMode.Put, base.AppLog);
		if (errorCodes != ErrorCodes.Success)
		{
			base.Context.CreateInfoResult("JobError", $"MOVE DECK={deck} SLOT={slot} MODE=PUT {errorCodes.ToString().ToUpper()}");
			return;
		}
		string matrix = "UNKNOWN";
		using (Scanner scanner = new Scanner(CenterDiskMethod.VendDoorAndBack, base.ControlSystem))
		{
			using ScanResult scanResult = scanner.Read();
			matrix = scanResult.ScannedMatrix;
		}
		IPutResult putResult = ServiceLocator.Instance.GetService<IControllerService>().Put(matrix);
		if (putResult.Success)
		{
			base.Context.CreateInfoResult("JobSuccess", $"PUT SUCCESS (barcode = {matrix})");
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
