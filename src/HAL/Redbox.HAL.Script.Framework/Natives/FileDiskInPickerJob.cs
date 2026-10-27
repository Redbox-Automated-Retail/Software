using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework.Natives;

[NativeJob(ProgramName = "file-disk-in-picker")]
internal sealed class FileDiskInPickerJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		IPickerSensorReadResult pickerSensorReadResult = base.ControlSystem.ReadPickerSensors();
		if (!pickerSensorReadResult.Success)
		{
			base.Context.CreateInfoResult("JobError", $"Read sensors returned error {pickerSensorReadResult.Error}");
			return;
		}
		if (!pickerSensorReadResult.IsFull)
		{
			base.Context.CreateInfoResult("JobError", "Gripper is empty.");
			return;
		}
		using IEmptySearchResult emptySearchResult = ServiceLocator.Instance.GetService<IEmptySearchPatternService>().FindEmptyLocations();
		if (emptySearchResult.FoundEmpty == 0)
		{
			base.Context.CreateInfoResult("JobError", "Empty slot not found.");
			return;
		}
		string matrix = Scanner.ReadDiskInPicker(ReadDiskOptions.LeaveCaptureResult | ReadDiskOptions.CheckForDuplicate, base.Context);
		ILocation location = emptySearchResult.EmptyLocations[0];
		if (!File(location, matrix))
		{
			ErrorCodes errorCodes = base.MotionService.InitAxes();
			base.Context.CreateInfoResult("InitStatus", string.Format("Init motors returned {0}", (errorCodes == ErrorCodes.Success) ? "SUCCESS" : "FAILURE"));
			if (errorCodes == ErrorCodes.Success)
			{
				File(location, matrix);
			}
		}
	}

	internal FileDiskInPickerJob(ExecutionResult r, ExecutionContext c)
		: base(r, c)
	{
	}

	private bool File(ILocation location, string matrix)
	{
		IPutToResult putToResult = ServiceLocator.Instance.GetService<IControllerService>().PutTo(matrix, location);
		if (putToResult.MoveResult != ErrorCodes.Success)
		{
			base.Context.CreateInfoResult("JobError", $"MOVE {location.ToString()} MODE=PUT {putToResult.MoveResult.ToString()}");
			return false;
		}
		if (putToResult.Success)
		{
			base.Context.CreateInfoResult("JobSuccess", $"Filed {location.ToString()}, id={matrix}");
			return true;
		}
		base.Context.CreateInfoResult("JobError", $"PUT failed at {location.ToString()}, status={putToResult.Code.ToString()}");
		return false;
	}
}
