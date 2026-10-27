using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal sealed class GetOperation : AbstractOperation<GetResult>
{
	private IFormattedLog Log;

	private readonly ILocation Location;

	private readonly IGetObserver Observer;

	private readonly IDeck Deck;

	private readonly int Slot;

	private readonly GetResult Result;

	public override GetResult Execute()
	{
		if (Validate())
		{
			FetchDisk();
		}
		return Result;
	}

	protected override void OnDispose()
	{
		Log = null;
	}

	internal GetOperation(ILocation location, IGetObserver observer, IFormattedLog log)
	{
		Observer = observer;
		Log = log;
		IDecksService service = ServiceLocator.Instance.GetService<IDecksService>();
		Deck = service.GetFrom(location);
		Slot = location.Slot;
		Location = location;
		Result = new GetResult(Location);
	}

	private void FetchDisk()
	{
		if (Location.IsWide)
		{
			SettleDiskInSlot();
		}
		if (!Controller.TrackOpen().Success)
		{
			Result.Update(ErrorCodes.TrackOpenTimeout);
			return;
		}
		Controller.StartRollerOut();
		ErrorCodes errorCodes = PullFrom(Location);
		if (errorCodes != ErrorCodes.Success)
		{
			Controller.StopRoller();
			Controller.TrackClose();
			Result.Update(errorCodes);
			return;
		}
		if (!Controller.TrackClose().Success)
		{
			Controller.StopRoller();
			PushIntoSlot();
			Result.Update(ErrorCodes.TrackCloseTimeout);
			return;
		}
		IPickerSensorReadResult pickerSensorReadResult = Controller.ReadPickerSensors();
		if (!pickerSensorReadResult.Success)
		{
			Result.Update(pickerSensorReadResult.Error);
			return;
		}
		if (pickerSensorReadResult.IsInputActive(PickerInputs.Sensor1) && !pickerSensorReadResult.IsInputActive(PickerInputs.Sensor2))
		{
			errorCodes = PullFrom(Location, 1);
		}
		RuntimeService.SpinWait(300);
		IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
		if (Controller.RollerToPosition(RollerPosition.Position4, 6000, logSensors: false).Success)
		{
			service.Reset(Location);
			return;
		}
		IPickerSensorReadResult pickerSensorReadResult2 = Controller.ReadPickerSensors();
		if (!pickerSensorReadResult2.Success)
		{
			Result.Update(ErrorCodes.SensorReadError);
			return;
		}
		if (pickerSensorReadResult2.IsFull)
		{
			LogHelper.Instance.WithContext(toFormattedLog: false, LogEntryType.Info, "[GET] Disk did not make it to sensor 4.");
			pickerSensorReadResult2.Log(LogEntryType.Error);
			OnStuck();
			return;
		}
		LogHelper.Instance.WithContext(toFormattedLog: false, LogEntryType.Info, "[GET] no disk in picker after pull.");
		pickerSensorReadResult2.Log();
		Result.Update(ErrorCodes.SlotEmpty);
		if (Observer.OnEmpty(Result))
		{
			service.Reset(Location);
		}
	}

	private void OnStuck()
	{
		Controller.StartRollerIn();
		try
		{
			bool flag = ClearDiskFromPicker();
			PushIntoSlot();
			LogHelper.Instance.WithContext(false, LogEntryType.Error, "[GET] couldn't get disc, ClearDiscFromPicker returned {0}", flag ? "TIMEOUT" : "SUCCESS");
			IPickerSensorReadResult pickerSensorReadResult = Controller.ReadPickerSensors();
			pickerSensorReadResult.Log(LogEntryType.Error);
			if (!pickerSensorReadResult.Success)
			{
				Result.Update(ErrorCodes.SensorReadError);
			}
			else
			{
				Result.Update(pickerSensorReadResult.IsFull ? ErrorCodes.PickerObstructed : ErrorCodes.ItemStuck);
			}
		}
		finally
		{
			Controller.StopRoller();
		}
		Observer.OnStuck(Result);
	}

	private bool Validate()
	{
		if (Controller.TrackState != TrackState.Closed && !Controller.TrackClose().Success)
		{
			Result.Update(ErrorCodes.TrackCloseTimeout);
			return false;
		}
		if (Controller.ReadPickerSensors().IsFull)
		{
			Result.Update(ErrorCodes.PickerFull);
			return false;
		}
		return true;
	}
}
