using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal sealed class PutOperation : AbstractOperation<PutResult>
{
	private readonly string ID;

	private readonly IPutObserver Observer;

	private readonly ILocation PutLocation;

	private readonly PutResult Result;

	private readonly IFormattedLog Log;

	private const string AggressivePutFailuresCounter = "AggressivePutFailures";

	private const string AggressivePutSuccessCounter = "AggressivePutSuccesses";

	public override PutResult Execute()
	{
		Result.Code = OnCorePut();
		if (Result.Code != ErrorCodes.Success)
		{
			Observer.OnFailedPut(Result, Log);
		}
		else
		{
			Observer.OnSuccessfulPut(Result, Log);
		}
		return Result;
	}

	internal PutOperation(string id, IPutObserver observer, ILocation putLocation, IFormattedLog log)
	{
		Result = new PutResult(id, putLocation);
		Log = log;
		PutLocation = putLocation;
		ID = id;
		Observer = observer;
	}

	private ErrorCodes OnCorePut()
	{
		if (PutLocation.IsWide)
		{
			SettleDiskInSlot();
		}
		if (Controller.TrackState != TrackState.Closed && !Controller.TrackClose().Success)
		{
			return ErrorCodes.TrackCloseTimeout;
		}
		if (!Controller.RetractArm().Success)
		{
			return ErrorCodes.GripperRetractTimeout;
		}
		if (!Controller.SetFinger(GripperFingerState.Rent).Success)
		{
			LogHelper.Instance.Log("PUT: SetGripperFinger to Rent timed out.", LogEntryType.Error);
			return ErrorCodes.GripperRentTimeout;
		}
		Controller.StartRollerIn();
		bool num = ClearDiskFromPicker();
		RuntimeService.SpinWait(100);
		if (num)
		{
			LogHelper.Instance.WithContext(LogEntryType.Error, "PUT: Disk cannot be placed into {0} because it cannot clear sensor 2.", PutLocation.ToString());
			Controller.LogPickerSensorState(LogEntryType.Error);
			Controller.StopRoller();
			RuntimeService.SpinWait(100);
			if (!Controller.TrackOpen().Success)
			{
				return ErrorCodes.TrackOpenTimeout;
			}
			Controller.TrackClose();
			RuntimeService.SpinWait(100);
			if (!Controller.RollerToPosition(RollerPosition.Position5, 8000).Success)
			{
				LogHelper.Instance.Log("Put: roll item back to sensor 5 returned TIMEOUT");
				LogHelper.Instance.Log("PUT: unable to roll item into slot, and cannot move it back into the picker.", LogEntryType.Error);
				Controller.LogPickerSensorState(LogEntryType.Error);
				return ErrorCodes.PickerObstructed;
			}
			return OnSlotInUse();
		}
		if (LogHelper.Instance.IsLevelEnabled(LogEntryType.Debug))
		{
			LogHelper.Instance.Log("Sensor wait for 2, 5 didn't time out.", LogEntryType.Info);
		}
		ErrorCodes errorCodes = PushIntoSlot();
		if (errorCodes != ErrorCodes.Success)
		{
			LogHelper.Instance.Log(LogEntryType.Error, "PUT: item_cleared_gripper: PushIntoSlot returned {0}", errorCodes.ToString().ToUpper());
		}
		Controller.StopRoller();
		IPickerSensorReadResult pickerSensorReadResult = Controller.ReadPickerSensors();
		if (!pickerSensorReadResult.Success)
		{
			return ErrorCodes.SensorReadError;
		}
		if (!pickerSensorReadResult.IsInputActive(PickerInputs.Sensor1))
		{
			UpdateInventory();
			return ErrorCodes.Success;
		}
		LogHelper.Instance.Log("[PUT] can't put disk in slot.", LogEntryType.Error);
		Controller.LogPickerSensorState(LogEntryType.Error);
		if (ControllerConfiguration.Instance.AggressiveClearPickerOnPut)
		{
			return OnAggressivePut();
		}
		if (!Controller.TrackOpen().Success)
		{
			return ErrorCodes.TrackOpenTimeout;
		}
		PullFrom(PutLocation);
		if (!Controller.TrackClose().Success)
		{
			return ErrorCodes.TrackCloseTimeout;
		}
		if (!Controller.RollerToPosition(RollerPosition.Position5, 5000).Success)
		{
			Controller.LogPickerSensorState(LogEntryType.Error);
			RuntimeService.SpinWait(100);
			Controller.StopRoller();
			return ErrorCodes.PickerObstructed;
		}
		IPickerSensorReadResult pickerSensorReadResult2 = Controller.ReadPickerSensors();
		if (pickerSensorReadResult2.IsInputActive(PickerInputs.Sensor1) || pickerSensorReadResult2.IsInputActive(PickerInputs.Sensor6))
		{
			return ErrorCodes.PickerObstructed;
		}
		return OnSlotInUse();
	}

	private ErrorCodes OnAggressivePut()
	{
		Controller.TrackCycle();
		Controller.StartRollerIn();
		PushIntoSlot();
		Controller.StopRoller();
		IPickerSensorReadResult pickerSensorReadResult = Controller.ReadPickerSensors();
		if (!pickerSensorReadResult.Success)
		{
			return ErrorCodes.SensorReadError;
		}
		if (!pickerSensorReadResult.IsInputActive(PickerInputs.Sensor1))
		{
			UpdateAggressivePutCounter();
			UpdateInventory();
			return ErrorCodes.Success;
		}
		Controller.TrackOpen();
		PushIntoSlot();
		Controller.TrackClose();
		pickerSensorReadResult = Controller.ReadPickerSensors();
		if (!pickerSensorReadResult.Success)
		{
			return ErrorCodes.SensorReadError;
		}
		if (!pickerSensorReadResult.IsInputActive(PickerInputs.Sensor1))
		{
			UpdateInventory();
			UpdateAggressivePutCounter();
			return ErrorCodes.Success;
		}
		LogHelper.Instance.Log("AggressivePut not able to clear the disk.", LogEntryType.Error);
		Controller.LogPickerSensorState(LogEntryType.Error);
		PersistentCounterService.Increment("AggressivePutFailures");
		Controller.StartRollerOut();
		PullFrom(PutLocation);
		IControlResponse controlResponse = Controller.RollerToPosition(RollerPosition.Position5, 5000);
		LogHelper.Instance.Log(LogEntryType.Error, "[PUT] gripperClear: false, rollerTo 5 returned {0}", controlResponse.ToString());
		Controller.ReadPickerSensors().Log(LogEntryType.Error);
		RuntimeService.SpinWait(100);
		Controller.StopRoller();
		return ErrorCodes.PickerObstructed;
	}

	private ErrorCodes OnSlotInUse()
	{
		PutLocation.ID = "UNKNOWN";
		InventoryService.Save(PutLocation);
		return ErrorCodes.SlotInUse;
	}

	private void UpdateAggressivePutCounter()
	{
		LogHelper.Instance.Log("Aggressive PUT was able to clear disk.", LogEntryType.Info);
		PersistentCounterService.Increment("AggressivePutSuccesses");
	}

	private void UpdateInventory()
	{
		ILocation original = null;
		Result.IsDuplicate = InventoryService.IsBarcodeDuplicate(ID, out original);
		Result.StoredMatrix = (IsSuspiciousId(ID) ? "UNKNOWN" : ID);
		if (PutLocation.Deck == 7 && PutLocation.IsWide)
		{
			Result.StoredMatrix = "UNKNOWN";
		}
		if (Result.IsDuplicate)
		{
			ServiceLocator.Instance.GetService<IExecutionService>().GetActiveContext();
			Result.OriginalMatrixLocation = original;
			Result.StoredMatrix = (ControllerConfiguration.Instance.MarkDuplicatesUnknown ? "UNKNOWN" : string.Format("{0}{1}", ID, "(DUPLICATE)"));
			LogHelper.Instance.WithContext("The ID {0} is a duplicate ( original at Deck = {1} Slot = {2} ); marking as {3}", ID, original.Deck, original.Slot, Result.StoredMatrix);
			if (ControllerConfiguration.Instance.MarkOriginalMatrixUnknown)
			{
				LogHelper.Instance.WithContext("Mark the original matrix ID = {0} at Deck = {1} Slot = {2} ); marking as {3}", ID, original.Deck, original.Slot, "UNKNOWN");
				original.ID = "UNKNOWN";
				InventoryService.Save(original);
			}
			PersistentCounterService.Increment("DUPLICATE-COUNT");
		}
		PutLocation.ID = Result.StoredMatrix;
		InventoryService.Save(PutLocation);
	}

	private bool IsSuspiciousId(string id)
	{
		if (InventoryConstants.IsKnownInventoryToken(id))
		{
			return false;
		}
		try
		{
			return Enum.IsDefined(typeof(ErrorCodes), id);
		}
		catch (Exception)
		{
			return false;
		}
	}
}
