using System.Collections.Generic;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework.Operations;
using Redbox.HAL.Controller.Framework.Services;

namespace Redbox.HAL.Controller.Framework;

public sealed class ControllerService : IMoveVeto, IControllerService
{
	private readonly DefaultPutObserver PutObserver = new DefaultPutObserver();

	private readonly DumpbinPutObserver DumpObserver = new DumpbinPutObserver();

	private readonly DefaultGetObserver GetObserver = new DefaultGetObserver();

	public ErrorCodes CanMove()
	{
		IControlSystem service = ServiceLocator.Instance.GetService<IControlSystem>();
		if (VendDoorState.Closed != service.VendDoorState && !service.VendDoorClose().Success)
		{
			return ErrorCodes.VendDoorNotClosed;
		}
		if (ClearGripper() != ErrorCodes.Success)
		{
			return ErrorCodes.ObstructionDetected;
		}
		IReadInputsResult<PickerInputs> readInputsResult = service.ReadPickerInputs();
		if (!readInputsResult.Success)
		{
			return ErrorCodes.SensorReadError;
		}
		if (!readInputsResult.IsInputActive(PickerInputs.Retract))
		{
			if (ControllerConfiguration.Instance.GripperRentOnMove && !readInputsResult.IsInputActive(PickerInputs.FingerRent) && !service.SetFinger(GripperFingerState.Rent).Success)
			{
				return ErrorCodes.GripperRentTimeout;
			}
			if (!service.RetractArm().Success)
			{
				return ErrorCodes.GripperRetractTimeout;
			}
		}
		if (ControllerConfiguration.Instance.CheckGripperArmSensorsOnMove && readInputsResult.IsInputActive(PickerInputs.Extend) && readInputsResult.IsInputActive(PickerInputs.Retract))
		{
			LogHelper.Instance.WithContext(toFormattedLog: false, LogEntryType.Error, "MOVE: can't move because extend/retract both triggered.");
			readInputsResult.Log(LogEntryType.Error);
			return ErrorCodes.SensorError;
		}
		return ErrorCodes.Success;
	}

	public void Initialize(ErrorList errors, IDictionary<string, object> initProperties)
	{
		ControllerConfiguration.Instance.Initialize();
		IDataTableService service = ServiceLocator.Instance.GetService<IDataTableService>();
		ServiceLocator.Instance.AddService<IKioskConfiguration>(new KioskConfigurationService());
		IInventoryService inventoryService = new InventoryService();
		ServiceLocator.Instance.AddService<IInventoryService>(inventoryService);
		PersistentCounterService persistentCounterService = new PersistentCounterService(service);
		ServiceLocator.Instance.AddService<IPersistentCounterService>(persistentCounterService);
		ServiceLocator.Instance.AddService<IHardwareCorrectionStatisticService>(new HardwareCorrectionStatisticService(service));
		ServiceLocator.Instance.AddService<IKioskFunctionCheckService>(new KioskFunctionCheckService());
		ServiceLocator.Instance.AddService<IEmptySearchPatternService>(new EmptySearchPatternService(inventoryService));
		MotionControlService_v2 motionControlService_v = new MotionControlService_v2();
		ServiceLocator.Instance.AddService<IMotionControlService>(motionControlService_v);
		motionControlService_v.AddVeto(this);
		IRuntimeService service2 = ServiceLocator.Instance.GetService<IRuntimeService>();
		ControlSystemBridgeModern instance = new ControlSystemBridgeModern(service2, persistentCounterService);
		ServiceLocator.Instance.AddService<IControlSystemService>(instance);
		ServiceLocator.Instance.AddService<IControlSystem>(instance);
		ServiceLocator.Instance.AddService<IAirExchangerService>(new IceQubeAirExchangerService());
		ServiceLocator.Instance.AddService<IDumpbinService>(new DumpbinServiceBridge());
		ServiceLocator.Instance.AddService<IDoorSensorService>(new DoorSensorService());
		ServiceLocator.Instance.AddService<IPowerCycleDeviceService>(new PowerCycleDeviceService(service2));
	}

	public void Shutdown()
	{
		ServiceLocator.Instance.GetService<IMotionControlService>().Shutdown();
		ServiceLocator.Instance.GetService<IControlSystem>().Shutdown();
	}

	public IGetResult Get()
	{
		return Get(GetObserver);
	}

	public IGetResult Get(IGetObserver observer)
	{
		IMotionControlService service = ServiceLocator.Instance.GetService<IMotionControlService>();
		return OnGet(observer, service.CurrentLocation);
	}

	public IGetFromResult GetFrom(ILocation location)
	{
		return GetFrom(GetObserver, location);
	}

	public IGetFromResult GetFrom(IGetFromObserver observer, ILocation location)
	{
		IMotionControlService service = ServiceLocator.Instance.GetService<IMotionControlService>();
		GetFromResult getFromResult = new GetFromResult();
		getFromResult.MoveResult = service.MoveTo(location, MoveMode.Get);
		if (getFromResult.MoveResult == ErrorCodes.Success)
		{
			getFromResult.GetResult = OnGet(observer, location);
		}
		else
		{
			observer?.OnMoveError(getFromResult.MoveResult);
		}
		return getFromResult;
	}

	public IPutResult Put(string id)
	{
		IDumpbinService service = ServiceLocator.Instance.GetService<IDumpbinService>();
		IMotionControlService service2 = ServiceLocator.Instance.GetService<IMotionControlService>();
		IPutObserver observer = PutObserver;
		if (service.IsBin(service2.CurrentLocation))
		{
			observer = DumpObserver;
		}
		return OnPut(observer, id, service2.CurrentLocation);
	}

	public IPutResult Put(IPutObserver observer, string id)
	{
		IMotionControlService service = ServiceLocator.Instance.GetService<IMotionControlService>();
		return OnPut(observer, id, service.CurrentLocation);
	}

	public IPutToResult PutTo(string id, ILocation location)
	{
		IDumpbinService service = ServiceLocator.Instance.GetService<IDumpbinService>();
		IPutToObserver observer = PutObserver;
		if (service.IsBin(location))
		{
			observer = DumpObserver;
		}
		return PutTo(observer, id, location);
	}

	public IPutToResult PutTo(IPutToObserver observer, string id, ILocation location)
	{
		IMotionControlService service = ServiceLocator.Instance.GetService<IMotionControlService>();
		PutToResult putToResult = new PutToResult();
		putToResult.MoveResult = service.MoveTo(location, MoveMode.Put);
		if (putToResult.MoveResult == ErrorCodes.Success)
		{
			ServiceLocator.Instance.GetService<IControlSystem>().TrackCycle();
			putToResult.PutResult = OnPut(observer, id, location);
		}
		else
		{
			observer?.OnMoveError(putToResult.MoveResult);
		}
		return putToResult;
	}

	public ITransferResult Transfer(ILocation source, ILocation destination)
	{
		return Transfer(source, destination, preserveFlags: true);
	}

	public ITransferResult Transfer(ILocation source, ILocation destination, bool preserveFlags)
	{
		using TransferOperation transferOperation = new TransferOperation(this, source);
		transferOperation.PreserveFlagsOnPut = preserveFlags;
		return transferOperation.Transfer(destination);
	}

	public ITransferResult Transfer(ILocation source, IList<ILocation> destinations, IGetObserver observer)
	{
		return Transfer(source, destinations, observer, preserveFlags: true);
	}

	public ITransferResult Transfer(ILocation source, IList<ILocation> destinations, IGetObserver observer, bool preserveFlags)
	{
		using TransferOperation transferOperation = new TransferOperation(this, source);
		transferOperation.PreserveFlagsOnPut = preserveFlags;
		return transferOperation.Transfer(destinations, observer);
	}

	public ErrorCodes PushOut()
	{
		using PushOutDiskOperation pushOutDiskOperation = new PushOutDiskOperation(this);
		ErrorCodes errorCodes = pushOutDiskOperation.Execute();
		if (errorCodes != ErrorCodes.Success)
		{
			LogHelper.Instance.WithContext(LogEntryType.Error, "Push out disk returned error status {0}", errorCodes.ToString().ToUpper());
			ServiceLocator.Instance.GetService<IControlSystem>().LogPickerSensorState(LogEntryType.Error);
		}
		return errorCodes;
	}

	public ErrorCodes ClearGripper()
	{
		using ClearGripperOperation clearGripperOperation = new ClearGripperOperation();
		ErrorCodes errorCodes = clearGripperOperation.Execute();
		if (errorCodes != ErrorCodes.Success)
		{
			LogHelper.Instance.WithContext(LogEntryType.Error, "Clear gripper returned error status {0}", errorCodes.ToString().ToUpper());
			ServiceLocator.Instance.GetService<IControlSystem>().LogPickerSensorState(LogEntryType.Error);
		}
		return errorCodes;
	}

	public IVendItemResult VendItemInPicker()
	{
		return VendItemInPicker(ControllerConfiguration.Instance.VendDiskPollCount);
	}

	public IVendItemResult VendItemInPicker(int attempts)
	{
		using VendItemOperation vendItemOperation = new VendItemOperation(attempts);
		VendItemResult vendItemResult = vendItemOperation.Execute();
		if (!vendItemResult.Presented)
		{
			LogHelper.Instance.WithContext(LogEntryType.Error, "VendItemInPicker: The disk was not presented to the user from the vend door.");
		}
		else if (ErrorCodes.PickerFull == vendItemResult.Status || ErrorCodes.PickerEmpty == vendItemResult.Status)
		{
			LogHelper.Instance.WithContext(false, LogEntryType.Info, "Vend item in picker returned status {0}", vendItemResult.Status.ToString().ToUpper());
		}
		else
		{
			LogHelper.Instance.WithContext(LogEntryType.Error, "Vend item in picker returned error code {0}", vendItemResult.Status.ToString().ToUpper());
		}
		return vendItemResult;
	}

	public ErrorCodes AcceptDiskAtDoor()
	{
		using AcceptDiskOperation acceptDiskOperation = new AcceptDiskOperation();
		ErrorCodes errorCodes = acceptDiskOperation.Execute();
		if (ErrorCodes.PickerEmpty == errorCodes || ErrorCodes.PickerFull == errorCodes)
		{
			LogHelper.Instance.WithContext(false, LogEntryType.Info, "Accept disk in picker returned status {0}", errorCodes);
		}
		else
		{
			LogHelper.Instance.WithContext("Accept disk at door returned error {0}", errorCodes.ToString().ToUpper());
		}
		return errorCodes;
	}

	public ErrorCodes RejectDiskInPicker()
	{
		return RejectDiskInPicker(ControllerConfiguration.Instance.RejectAtDoorAttempts);
	}

	public ErrorCodes RejectDiskInPicker(int attempts)
	{
		using RejectDiskOperation rejectDiskOperation = new RejectDiskOperation(attempts);
		ErrorCodes errorCodes = rejectDiskOperation.Execute();
		if (ErrorCodes.PickerEmpty == errorCodes || ErrorCodes.PickerFull == errorCodes)
		{
			LogHelper.Instance.WithContext(false, LogEntryType.Info, "Reject disk returned status {0}", errorCodes.ToString().ToUpper());
		}
		else
		{
			LogHelper.Instance.WithContext(LogEntryType.Error, "Reject disk returned error {0}", errorCodes.ToString().ToUpper());
		}
		return errorCodes;
	}

	private void OnCheckInventory(string expected, IFormattedLog log)
	{
		IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
		ILocation currentLocation = ServiceLocator.Instance.GetService<IMotionControlService>().CurrentLocation;
		ILocation location = service.Get(currentLocation.Deck, currentLocation.Slot);
		if (location.ID != expected)
		{
			LogHelper.Instance.WithContext(false, LogEntryType.Error, "** INVENTORY CHECK ERROR** - location shows ID {0} expected {1}", location.ID, expected);
		}
	}

	private GetResult OnGet(IGetObserver observer, ILocation location)
	{
		if (location == null)
		{
			GetResult getResult = new GetResult(location);
			getResult.Update(ErrorCodes.LocationOutOfRange);
			return getResult;
		}
		IFormattedLog contextLog = ServiceLocator.Instance.GetService<IExecutionService>().GetActiveContext().ContextLog;
		using GetOperation getOperation = new GetOperation(location, observer, contextLog);
		GetResult getResult2 = getOperation.Execute();
		if (getResult2.IsSlotEmpty)
		{
			string fmt = $"GET {location.ToString()} returned SLOTEMPTY";
			LogHelper.Instance.WithContext(fmt);
			return getResult2;
		}
		if (!getResult2.Success)
		{
			string msg = $"GET {location.ToString()} returned error status {getResult2.ToString().ToUpper()}";
			LogHelper.Instance.WithContext(LogEntryType.Error, msg);
			return getResult2;
		}
		string msg2 = $"GET {location.ToString()} ID={getResult2.Previous}";
		contextLog.WriteFormatted(msg2);
		OnCheckInventory("EMPTY", contextLog);
		return getResult2;
	}

	private PutResult OnPut(IPutObserver observer, string id, ILocation location)
	{
		IControlSystem service = ServiceLocator.Instance.GetService<IControlSystem>();
		IPickerSensorReadResult pickerSensorReadResult = service.ReadPickerSensors();
		if (!pickerSensorReadResult.Success)
		{
			return new PutResult(id, location)
			{
				Code = ErrorCodes.SensorReadError
			};
		}
		if (!pickerSensorReadResult.IsFull)
		{
			LogHelper.Instance.WithContext(LogEntryType.Error, "PUT: picker is empty.");
			service.LogPickerSensorState();
			service.LogInputs(ControlBoards.Picker, LogEntryType.Info);
			return new PutResult(id, location)
			{
				Code = ErrorCodes.PickerEmpty
			};
		}
		string arg = (ServiceLocator.Instance.GetService<IDumpbinService>().IsBin(location) ? "DUMPBIN" : $"Deck = {location.Deck} Slot = {location.Slot}");
		IFormattedLog contextLog = ServiceLocator.Instance.GetService<IExecutionService>().GetActiveContext().ContextLog;
		using PutOperation putOperation = new PutOperation(id, observer, location, contextLog);
		PutResult putResult = putOperation.Execute();
		if (putResult.Success)
		{
			contextLog.WriteFormatted($"PUT {arg} ID={id}");
			OnCheckInventory(id, contextLog);
		}
		else
		{
			string msg = $"PUT {arg} ID={id} returned error status {putResult.ToString().ToUpper()}";
			LogHelper.Instance.WithContext(LogEntryType.Error, msg);
		}
		return putResult;
	}
}
