using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Timers;

namespace Redbox.HAL.Controller.Framework;

public abstract class AbstractOperation<T> : IDisposable
{
	protected readonly IInventoryService InventoryService;

	protected readonly IRuntimeService RuntimeService;

	protected readonly IControlSystem Controller;

	protected readonly IMotionControlService MotionService;

	protected readonly IDecksService DeckService;

	protected readonly IPersistentCounterService PersistentCounterService;

	protected readonly IControllerService ControllerService;

	private bool Disposed;

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	public abstract T Execute();

	protected bool WaitToClear()
	{
		return WaitToClear(8000);
	}

	protected bool WaitToClear(int timeout)
	{
		bool result = false;
		int clearSensorPauseDelay = ControllerConfiguration.Instance.ClearSensorPauseDelay;
		using (ExecutionTimer executionTimer = new ExecutionTimer())
		{
			while (true)
			{
				RuntimeService.SpinWait(clearSensorPauseDelay);
				IPickerSensorReadResult pickerSensorReadResult = Controller.ReadPickerSensors();
				if (!pickerSensorReadResult.Success)
				{
					return true;
				}
				if (ClearPickerBlockedCount(pickerSensorReadResult) == 0)
				{
					result = false;
					break;
				}
				if (executionTimer.ElapsedMilliseconds > timeout)
				{
					result = true;
					break;
				}
			}
		}
		return result;
	}

	protected bool ClearDiskFromPicker()
	{
		IRuntimeService service = ServiceLocator.Instance.GetService<IRuntimeService>();
		int clearSensorPauseDelay = ControllerConfiguration.Instance.ClearSensorPauseDelay;
		bool flag = WaitToClear(8000);
		if (!flag)
		{
			service.SpinWait(clearSensorPauseDelay);
			bool flag2 = true;
			IPickerSensorReadResult pickerSensorReadResult;
			for (int i = 0; (i < 3) & flag2; i++)
			{
				if (flag2)
				{
					if (i > 0)
					{
						LogHelper.Instance.Log("2-5 didn't timeout; sensors show something in picker.", LogEntryType.Info);
					}
					WaitToClear(3000);
				}
				pickerSensorReadResult = Controller.ReadPickerSensors();
				if (!pickerSensorReadResult.Success)
				{
					return true;
				}
				flag2 = ClearPickerBlockedCount(pickerSensorReadResult) > 0;
				service.SpinWait(clearSensorPauseDelay);
			}
			pickerSensorReadResult = Controller.ReadPickerSensors();
			if (!pickerSensorReadResult.Success)
			{
				return true;
			}
			flag = ClearPickerBlockedCount(pickerSensorReadResult) > 0;
		}
		return flag;
	}

	protected ErrorCodes WaitSensor(PickerInputs sensor, InputState state)
	{
		return WaitSensor(sensor, state, 4000);
	}

	protected ErrorCodes WaitSensor(PickerInputs sensor, InputState state, int timeout)
	{
		int waitSensorPauseTime = ControllerConfiguration.Instance.WaitSensorPauseTime;
		ErrorCodes errorCodes = ErrorCodes.Success;
		using (ExecutionTimer executionTimer = new ExecutionTimer())
		{
			while (true)
			{
				RuntimeService.SpinWait(waitSensorPauseTime);
				IPickerSensorReadResult pickerSensorReadResult = Controller.ReadPickerSensors();
				if (!pickerSensorReadResult.Success)
				{
					errorCodes = ErrorCodes.SensorReadError;
					break;
				}
				if (pickerSensorReadResult.IsInState(sensor, state))
				{
					break;
				}
				if (executionTimer.ElapsedMilliseconds > timeout)
				{
					errorCodes = ErrorCodes.Timeout;
					break;
				}
			}
		}
		if (errorCodes != ErrorCodes.Success && ErrorCodes.Timeout != errorCodes)
		{
			LogHelper.Instance.WithContext(LogEntryType.Error, "WaitSensor returned error {0}.", errorCodes.ToString().ToUpper());
		}
		return errorCodes;
	}

	protected ErrorCodes SettleDiskInSlot()
	{
		try
		{
			if (!Controller.SetFinger(GripperFingerState.Open).Success)
			{
				return ErrorCodes.GripperOpenTimeout;
			}
			if (Controller.ExtendArm().TimedOut)
			{
				Controller.RetractArm();
				Controller.SetFinger(GripperFingerState.Closed);
				Controller.SetFinger(GripperFingerState.Open);
				if (!Controller.ExtendArm().Success)
				{
					Controller.RetractArm();
					return ErrorCodes.GripperExtendTimeout;
				}
			}
			if (!Controller.SetFinger(GripperFingerState.Closed).Success)
			{
				return ErrorCodes.GripperCloseTimeout;
			}
			if (!Controller.SetFinger(GripperFingerState.Open).Success)
			{
				return ErrorCodes.GripperOpenTimeout;
			}
			return (!Controller.RetractArm().Success) ? ErrorCodes.GripperRetractTimeout : ErrorCodes.Success;
		}
		finally
		{
			Controller.SetFinger(GripperFingerState.Open);
			Controller.RetractArm();
		}
	}

	protected ErrorCodes PullFrom(ILocation location)
	{
		return PullFrom(location, ControllerConfiguration.Instance.NumberOfPulls);
	}

	protected ErrorCodes PullFrom(ILocation location, int pulls)
	{
		GripperFingerState gripperFingerState = ((!location.IsWide) ? GripperFingerState.Rent : GripperFingerState.Open);
		for (int i = 0; i < pulls; i++)
		{
			if (!Controller.SetFinger(gripperFingerState).Success)
			{
				if (gripperFingerState != GripperFingerState.Open)
				{
					return ErrorCodes.GripperRentTimeout;
				}
				return ErrorCodes.GripperOpenTimeout;
			}
			if (DeckService.GetByNumber(MotionService.CurrentLocation.Deck).IsQlm)
			{
				int qlmExtendTime = ControllerConfiguration.Instance.QlmExtendTime;
				if (ControllerConfiguration.Instance.QlmTimedExtend)
				{
					Controller.TimedExtend(qlmExtendTime);
				}
				else
				{
					Controller.ExtendArm(ControllerConfiguration.Instance.QlmExtendTime);
				}
			}
			else if (Controller.ExtendArm().TimedOut)
			{
				Controller.RetractArm();
				Controller.SetFinger(GripperFingerState.Closed);
				Controller.SetFinger(gripperFingerState);
				if (!Controller.ExtendArm().Success)
				{
					Controller.RetractArm();
					return ErrorCodes.GripperExtendTimeout;
				}
			}
			if (!Controller.SetFinger(GripperFingerState.Closed).Success)
			{
				return ErrorCodes.GripperCloseTimeout;
			}
			if (!Controller.RetractArm().Success)
			{
				return ErrorCodes.GripperRetractTimeout;
			}
		}
		if (!Controller.SetFinger(GripperFingerState.Rent).Success)
		{
			return ErrorCodes.GripperRentTimeout;
		}
		return ErrorCodes.Success;
	}

	protected ErrorCodes PushIntoSlot()
	{
		ErrorCodes errorCodes = PushWithArm(ControllerConfiguration.Instance.RollInExtendTime);
		if (errorCodes != ErrorCodes.Success)
		{
			return errorCodes;
		}
		errorCodes = PushWithArm(ControllerConfiguration.Instance.PushTime);
		if (errorCodes != ErrorCodes.Success)
		{
			return errorCodes;
		}
		if (ControllerConfiguration.Instance.AdditionalPutPush)
		{
			PushWithArm(ControllerConfiguration.Instance.PushTime);
		}
		return ErrorCodes.Success;
	}

	protected int ClearPickerBlockedCount(IPickerSensorReadResult readResult)
	{
		int num = 0;
		if (readResult.IsInputActive(PickerInputs.Sensor2))
		{
			num++;
		}
		if (readResult.IsInputActive(PickerInputs.Sensor3))
		{
			num++;
		}
		if (readResult.IsInputActive(PickerInputs.Sensor4))
		{
			num++;
		}
		if (readResult.IsInputActive(PickerInputs.Sensor5))
		{
			num++;
		}
		return num;
	}

	protected ErrorCodes OpenDoorChecked()
	{
		VendDoorState vendDoorState = Controller.VendDoorState;
		if (VendDoorState.Rent != vendDoorState && !Controller.VendDoorRent().Success)
		{
			return ErrorCodes.VendDoorRentTimeout;
		}
		return ErrorCodes.Success;
	}

	protected ErrorCodes CloseTrackChecked()
	{
		if (Controller.TrackState != TrackState.Closed && !Controller.TrackClose().Success)
		{
			return ErrorCodes.TrackCloseTimeout;
		}
		return ErrorCodes.Success;
	}

	protected virtual void OnDispose()
	{
	}

	protected AbstractOperation()
	{
		DeckService = ServiceLocator.Instance.GetService<IDecksService>();
		MotionService = ServiceLocator.Instance.GetService<IMotionControlService>();
		Controller = ServiceLocator.Instance.GetService<IControlSystem>();
		RuntimeService = ServiceLocator.Instance.GetService<IRuntimeService>();
		InventoryService = ServiceLocator.Instance.GetService<IInventoryService>();
		PersistentCounterService = ServiceLocator.Instance.GetService<IPersistentCounterService>();
		ControllerService = ServiceLocator.Instance.GetService<IControllerService>();
	}

	private ErrorCodes PushWithArm(int timeout)
	{
		if (!Controller.SetFinger(GripperFingerState.Closed).Success)
		{
			return ErrorCodes.GripperCloseTimeout;
		}
		Controller.TimedExtend(timeout);
		if (!Controller.SetFinger(GripperFingerState.Rent).Success)
		{
			return ErrorCodes.GripperRentTimeout;
		}
		if (Controller.RetractArm().Success)
		{
			return ErrorCodes.Success;
		}
		return ErrorCodes.GripperRetractTimeout;
	}

	private void Dispose(bool disposing)
	{
		if (!Disposed)
		{
			Disposed = true;
			if (disposing)
			{
				OnDispose();
			}
		}
	}
}
