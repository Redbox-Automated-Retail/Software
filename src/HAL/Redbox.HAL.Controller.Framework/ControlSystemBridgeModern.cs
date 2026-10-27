using System.Collections.Generic;
using System.Threading;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Threading;

namespace Redbox.HAL.Controller.Framework;

internal sealed class ControlSystemBridgeModern : IConfigurationObserver, IControlSystemService, IControlSystem
{
	private AbstractRedboxControlSystem Implementor;

	private readonly IPersistentCounterService PersistentCounterService;

	private readonly IRuntimeService RuntimeService;

	private readonly List<IControlSystemObserver> Observers = new List<IControlSystemObserver>();

	private readonly ReaderWriterLockSlim ObserverLock = new ReaderWriterLockSlim();

	public bool IsInitialized { get; private set; }

	public VendDoorState VendDoorState { get; private set; }

	public TrackState TrackState { get; private set; }

	public void NotifyConfigurationChangeStart()
	{
	}

	public void NotifyConfigurationChangeEnd()
	{
	}

	public void NotifyConfigurationLoaded()
	{
		LogHelper.Instance.Log("[ControlSystemBridge] Configuration load.");
		FmeControlSystem instance = (FmeControlSystem)(Implementor = new FmeControlSystem(RuntimeService));
		ServiceLocator.Instance.AddService<ICoreCommandExecutor>(instance);
	}

	public void AddHandler(IControlSystemObserver observer)
	{
		using (new WithWriteLock(ObserverLock))
		{
			if (!Observers.Contains(observer))
			{
				Observers.Add(observer);
			}
		}
	}

	public void RemoveHandler(IControlSystemObserver observer)
	{
		using (new WithWriteLock(ObserverLock))
		{
			Observers.Remove(observer);
		}
	}

	public bool Restart()
	{
		if (!Shutdown())
		{
			return false;
		}
		RuntimeService.Wait(1200);
		return Initialize().Success;
	}

	public IControlResponse Initialize()
	{
		CoreResponse coreResponse = Implementor.Initialize();
		IsInitialized = coreResponse.Success;
		if (coreResponse.Success)
		{
			using (new WithReadLock(ObserverLock))
			{
				Observers.ForEach(delegate(IControlSystemObserver each)
				{
					each.OnSystemInitialize(ErrorCodes.Success);
				});
			}
			VendDoorState = Implementor.ReadVendDoorState();
		}
		return coreResponse;
	}

	public bool Shutdown()
	{
		using (new WithReadLock(ObserverLock))
		{
			Observers.ForEach(delegate(IControlSystemObserver observer)
			{
				observer.OnSystemShutdown();
			});
		}
		IsInitialized = false;
		return Implementor.Shutdown();
	}

	public IControlResponse SetAudio(AudioChannelState newState)
	{
		return Implementor.SetAudio(newState);
	}

	public IControlResponse ToggleRingLight(bool on, int? sleepAfter)
	{
		CoreResponse coreResponse = Implementor.SetRinglight(on);
		if (coreResponse.Success && sleepAfter.HasValue)
		{
			RuntimeService.Wait(sleepAfter.Value);
		}
		return coreResponse;
	}

	public IControlResponse VendDoorRent()
	{
		VendDoorState = VendDoorState.Unknown;
		CoreResponse coreResponse = Implementor.SetVendDoor(VendDoorState.Rent);
		if (!coreResponse.Success)
		{
			PersistentCounterService.Increment(TimeoutCounters.VendDoorRent);
			return coreResponse;
		}
		VendDoorState = VendDoorState.Rent;
		return coreResponse;
	}

	public IControlResponse VendDoorClose()
	{
		VendDoorState = VendDoorState.Unknown;
		CoreResponse coreResponse = Implementor.SetVendDoor(VendDoorState.Closed);
		if (!coreResponse.Success)
		{
			PersistentCounterService.Increment(TimeoutCounters.VendDoorClose);
			return coreResponse;
		}
		VendDoorState = VendDoorState.Closed;
		return coreResponse;
	}

	public VendDoorState ReadVendDoorPosition()
	{
		return Implementor.ReadVendDoorState();
	}

	public IControlResponse TrackOpen()
	{
		TrackState = TrackState.Unknown;
		CoreResponse coreResponse = Implementor.SetTrack(TrackState.Open);
		if (coreResponse.Success)
		{
			TrackState = TrackState.Open;
			return coreResponse;
		}
		PersistentCounterService.Increment(TimeoutCounters.TrackOpen);
		return coreResponse;
	}

	public IControlResponse TrackClose()
	{
		TrackState = TrackState.Unknown;
		CoreResponse coreResponse = Implementor.SetTrack(TrackState.Closed);
		if (coreResponse.Success)
		{
			TrackState = TrackState.Closed;
			return coreResponse;
		}
		PersistentCounterService.Increment(TimeoutCounters.TrackClose);
		return coreResponse;
	}

	public ErrorCodes TrackCycle()
	{
		if (!TrackOpen().Success)
		{
			return ErrorCodes.TrackOpenTimeout;
		}
		if (!TrackClose().Success)
		{
			return ErrorCodes.TrackCloseTimeout;
		}
		return ErrorCodes.Success;
	}

	public void TimedExtend()
	{
		TimedExtend(ControllerConfiguration.Instance.PushTime);
	}

	public void TimedExtend(int timeout)
	{
		Implementor.TimedArmExtend(timeout);
	}

	public IControlResponse ExtendArm()
	{
		int timeout = ControllerConfiguration.Instance.GripperArmExtendRetractTimeout;
		IDecksService service = ServiceLocator.Instance.GetService<IDecksService>();
		ILocation currentLocation = ServiceLocator.Instance.GetService<IMotionControlService>().CurrentLocation;
		bool flag = false;
		if (currentLocation != null)
		{
			flag = service.GetByNumber(currentLocation.Deck).IsQlm;
		}
		if (flag)
		{
			timeout = ControllerConfiguration.Instance.QlmExtendTime;
		}
		return ExtendArm(timeout);
	}

	public IControlResponse ExtendArm(int timeout)
	{
		CoreResponse coreResponse = Implementor.ExtendArm(timeout);
		if (!coreResponse.Success)
		{
			PersistentCounterService.Increment(TimeoutCounters.GripperExtend);
		}
		return coreResponse;
	}

	public IControlResponse RetractArm()
	{
		int gripperArmExtendRetractTimeout = ControllerConfiguration.Instance.GripperArmExtendRetractTimeout;
		CoreResponse coreResponse = Implementor.RetractArm(gripperArmExtendRetractTimeout);
		if (!coreResponse.Success)
		{
			PersistentCounterService.Increment(TimeoutCounters.GripperRetract);
		}
		return coreResponse;
	}

	public IControlResponse SetFinger(GripperFingerState state)
	{
		CoreResponse coreResponse = Implementor.SetFinger(state);
		if (!coreResponse.Success)
		{
			IPersistentCounterService service = ServiceLocator.Instance.GetService<IPersistentCounterService>();
			switch (state)
			{
			case GripperFingerState.Closed:
				service.Increment(TimeoutCounters.FingerClose);
				return coreResponse;
			case GripperFingerState.Open:
				service.Increment(TimeoutCounters.FingerOpen);
				return coreResponse;
			}
			service.Increment(TimeoutCounters.FingerRent);
		}
		return coreResponse;
	}

	public ErrorCodes Center(CenterDiskMethod method)
	{
		ErrorCodes result = ErrorCodes.Success;
		if (method == CenterDiskMethod.None)
		{
			return result;
		}
		IRuntimeService service = ServiceLocator.Instance.GetService<IRuntimeService>();
		int milliseconds = 250;
		RollerPosition rollerPosition = ((CenterDiskMethod.DrumAndBack == method) ? RollerPosition.Position1 : RollerPosition.Position6);
		if (!RollerToPosition(rollerPosition, ControllerConfiguration.Instance.DefaultRollSensorTimeout, logSensors: false).Success)
		{
			result = ((RollerPosition.Position1 == rollerPosition) ? ErrorCodes.RollerToPos1Timeout : ErrorCodes.RollerToPos6Timeout);
			LogHelper.Instance.WithContext(false, LogEntryType.Error, "Center disk: Roller {0} timed out.", rollerPosition);
		}
		service.SpinWait(milliseconds);
		rollerPosition = ((CenterDiskMethod.DrumAndBack == method) ? RollerPosition.Position5 : RollerPosition.Position3);
		if (!RollerToPosition(rollerPosition, ControllerConfiguration.Instance.DefaultRollSensorTimeout, logSensors: false).Success)
		{
			result = ((RollerPosition.Position5 == rollerPosition) ? ErrorCodes.RollerToPos5Timeout : ErrorCodes.RollerToPos3Timeout);
			LogHelper.Instance.WithContext(false, LogEntryType.Error, "Center disk: Roller {0} timed out.", rollerPosition);
		}
		service.SpinWait(milliseconds);
		TrackCycle();
		return result;
	}

	public IBoardVersionResponse GetBoardVersion(ControlBoards board)
	{
		return Implementor.GetBoardVersion(board);
	}

	public IControlSystemRevision GetRevision()
	{
		return Implementor.GetRevision();
	}

	public IReadInputsResult<PickerInputs> ReadPickerInputs()
	{
		ReadPickerInputsResult readPickerInputsResult = Implementor.ReadPickerInputs();
		if (!readPickerInputsResult.Success)
		{
			LogHelper.Instance.WithContext(LogEntryType.Error, "Read Picker inputs failed with error {0}", readPickerInputsResult.Error);
		}
		return readPickerInputsResult;
	}

	public IReadInputsResult<AuxInputs> ReadAuxInputs()
	{
		ReadAuxInputsResult readAuxInputsResult = Implementor.ReadAuxInputs();
		if (!readAuxInputsResult.Success)
		{
			LogHelper.Instance.WithContext(LogEntryType.Error, "Read AUX inputs failed with error {0}", readAuxInputsResult.Error);
		}
		return readAuxInputsResult;
	}

	public void LogPickerSensorState()
	{
		LogPickerSensorState(LogEntryType.Info);
	}

	public void LogPickerSensorState(LogEntryType type)
	{
		ReadPickerSensors().Log(type);
	}

	public void LogInputs(ControlBoards board)
	{
		LogInputs(board, LogEntryType.Info);
	}

	public void LogInputs(ControlBoards board, LogEntryType type)
	{
		if (ControlBoards.Picker == board)
		{
			Implementor.ReadPickerInputs().Log(type);
		}
		else
		{
			Implementor.ReadAuxInputs().Log(type);
		}
	}

	public IControlResponse SetSensors(bool on)
	{
		return Implementor.SetPickerSensors(on);
	}

	public IPickerSensorReadResult ReadPickerSensors()
	{
		return ReadPickerSensors(closeTrack: true);
	}

	public IPickerSensorReadResult ReadPickerSensors(bool closeTrack)
	{
		if (closeTrack && TrackState.Closed != TrackState && !TrackClose().Success)
		{
			return new PickerSensorReadResult(ErrorCodes.TrackCloseTimeout);
		}
		try
		{
			CoreResponse coreResponse = Implementor.SetPickerSensors(on: true);
			if (!coreResponse.Success)
			{
				return new PickerSensorReadResult(coreResponse.Error);
			}
			int pickerSensorSpinTime = ControllerConfiguration.Instance.PickerSensorSpinTime;
			RuntimeService.SpinWait(pickerSensorSpinTime);
			ReadPickerInputsResult result = Implementor.ReadPickerInputs();
			RuntimeService.SpinWait(pickerSensorSpinTime);
			return new PickerSensorReadResult(result);
		}
		finally
		{
			Implementor.SetPickerSensors(on: false);
		}
	}

	public IControlResponse StartRollerIn()
	{
		return SetRollerState(RollerState.In);
	}

	public IControlResponse StartRollerOut()
	{
		return SetRollerState(RollerState.Out);
	}

	public IControlResponse StopRoller()
	{
		return SetRollerState(RollerState.Stop);
	}

	public IControlResponse SetRollerState(RollerState state)
	{
		return Implementor.SetRoller(state);
	}

	public IControlResponse RollerToPosition(RollerPosition position)
	{
		return RollerToPosition(position, ControllerConfiguration.Instance.DefaultRollSensorTimeout);
	}

	public IControlResponse RollerToPosition(RollerPosition position, int opTimeout)
	{
		return RollerToPosition(position, opTimeout, logSensors: true);
	}

	public IControlResponse RollerToPosition(RollerPosition position, int opTimeout, bool logSensors)
	{
		CoreResponse coreResponse = Implementor.RollerToPosition(position, opTimeout);
		if (coreResponse.Success)
		{
			return coreResponse;
		}
		if (logSensors && !coreResponse.CommError)
		{
			LogHelper.Instance.WithContext(false, LogEntryType.Error, "Roller to {0} timed out.", position.ToString());
			LogPickerSensorState(LogEntryType.Error);
		}
		return coreResponse;
	}

	public QlmStatus GetQlmStatus()
	{
		return Implementor.GetQlmStatus();
	}

	public ErrorCodes EngageQlm(IFormattedLog log)
	{
		return EngageQlm(home: true, log);
	}

	public ErrorCodes EngageQlm(bool home, IFormattedLog log)
	{
		return OnLifterOperation(QlmOperation.Engage, log, home);
	}

	public ErrorCodes DisengageQlm(IFormattedLog log)
	{
		return DisengageQlm(home: true, log);
	}

	public ErrorCodes DisengageQlm(bool home, IFormattedLog log)
	{
		return OnLifterOperation(QlmOperation.Disengage, log, home);
	}

	public IControlResponse LockQlmDoor()
	{
		return Implementor.OnQlm(QlmOperation.LockDoor);
	}

	public IControlResponse UnlockQlmDoor()
	{
		return Implementor.OnQlm(QlmOperation.UnlockDoor);
	}

	public IControlResponse DropQlm()
	{
		return Implementor.OnQlm(QlmOperation.Drop);
	}

	public IControlResponse LiftQlm()
	{
		return Implementor.OnQlm(QlmOperation.Lift);
	}

	public IControlResponse HaltQlmLifter()
	{
		return Implementor.OnQlm(QlmOperation.Halt);
	}

	internal ControlSystemBridgeModern(IRuntimeService rts, IPersistentCounterService pcs)
	{
		ControllerConfiguration.Instance.AddObserver(this);
		IsInitialized = false;
		RuntimeService = rts;
		PersistentCounterService = pcs;
	}

	private ErrorCodes OnLifterOperation(QlmOperation operation, IFormattedLog log, bool home)
	{
		if (ControllerConfiguration.Instance.IsVMZMachine)
		{
			return ErrorCodes.Timeout;
		}
		if (home && ServiceLocator.Instance.GetService<IMotionControlService>().HomeAxis(Axis.X) != ErrorCodes.Success)
		{
			return ErrorCodes.HomeXTimeout;
		}
		CoreResponse coreResponse = Implementor.OnQlm(operation);
		ErrorCodes result = ((!coreResponse.Success) ? ErrorCodes.Timeout : ErrorCodes.Success);
		string msg = $"{operation.ToString().ToUpper()} returned status {result.ToString().ToUpper()}.";
		log.WriteFormatted(msg);
		if (!coreResponse.Success)
		{
			PersistentCounterService.Increment((QlmOperation.Engage == operation) ? TimeoutCounters.QlmEngage : TimeoutCounters.QlmDisengage);
		}
		return result;
	}
}
