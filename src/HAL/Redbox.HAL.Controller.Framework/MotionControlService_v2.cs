using System.Collections.Generic;
using System.Threading;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Threading;

namespace Redbox.HAL.Controller.Framework;

internal sealed class MotionControlService_v2 : IConfigurationObserver, IMotionControlService
{
	private readonly ReaderWriterLockSlim Lock = new ReaderWriterLockSlim();

	private readonly List<IMoveVeto> Vetoers = new List<IMoveVeto>();

	private AbstractMotionController Controller;

	private bool YAxisHomed;

	private bool XAxisHomed;

	public ILocation CurrentLocation { get; private set; }

	public bool AtVendDoor { get; private set; }

	public bool IsInitialized => AxesInitialized() == ErrorCodes.Success;

	public void NotifyConfigurationLoaded()
	{
		Controller = new ArcusMotionController2();
		Controller.OnConfigurationLoad();
	}

	public void NotifyConfigurationChangeStart()
	{
		Controller.OnConfigurationChangeStart();
	}

	public void NotifyConfigurationChangeEnd()
	{
		Controller.OnConfigurationChangeEnd();
	}

	public void AddVeto(IMoveVeto veto)
	{
		using (new WithWriteLock(Lock))
		{
			Vetoers.Add(veto);
		}
	}

	public void RemoveVeto(IMoveVeto veto)
	{
		using (new WithWriteLock(Lock))
		{
			Vetoers.Remove(veto);
		}
	}

	public bool Initialize()
	{
		return Controller.OnStartup();
	}

	public bool CommunicationOk()
	{
		return Controller.CommunicationOk();
	}

	public IControllerPosition ReadPositions()
	{
		return Controller.ReadPositions();
	}

	public IMotionControlLimitResponse ReadLimits()
	{
		return Controller.ReadLimits();
	}

	public ErrorCodes MoveAbsolute(Axis axis, int? xunits, int? yunits, bool checkSensors)
	{
		if (checkSensors)
		{
			ErrorCodes errorCodes = AxesInitialized();
			if (errorCodes != ErrorCodes.Success)
			{
				return errorCodes;
			}
		}
		MoveTarget target = new MoveTarget
		{
			Axis = axis,
			XCoordinate = xunits,
			YCoordinate = yunits
		};
		if (checkSensors)
		{
			return MoveWithCheck(ref target);
		}
		return Controller.MoveToTarget(ref target);
	}

	public ErrorCodes MoveTo(int deck, int slot, MoveMode mode)
	{
		IExecutionService service = ServiceLocator.Instance.GetService<IExecutionService>();
		return MoveTo(deck, slot, mode, service.GetActiveContext().ContextLog);
	}

	public ErrorCodes MoveTo(int deck, int slot, MoveMode mode, IFormattedLog log)
	{
		OffsetMoveData data = default(OffsetMoveData);
		return MoveTo(deck, slot, mode, log, ref data);
	}

	public ErrorCodes MoveTo(int deck, int slot, MoveMode mode, IFormattedLog _log, ref OffsetMoveData data)
	{
		IDeck byNumber = ServiceLocator.Instance.GetService<IDecksService>().GetByNumber(deck);
		if (byNumber == null)
		{
			return ErrorCodes.DeckOutOfRange;
		}
		if (!byNumber.IsSlotValid(slot))
		{
			return ErrorCodes.SlotOutOfRange;
		}
		ErrorCodes errorCodes = AxesInitialized();
		if (errorCodes != ErrorCodes.Success)
		{
			LogMoveError(deck, slot, errorCodes);
			return errorCodes;
		}
		int num = byNumber.YOffset;
		int num2 = (byNumber.IsQlm ? ControllerConfiguration.Instance.QlmYOffset : 50);
		switch (mode)
		{
		case MoveMode.Put:
			num += num2 * (int)ControllerConfiguration.Instance.GearY.GetStepRatio();
			break;
		case MoveMode.Get:
			num -= num2 * (int)ControllerConfiguration.Instance.GearY.GetStepRatio();
			break;
		}
		int num3 = byNumber.GetSlotOffset(slot);
		if (byNumber.IsQlm)
		{
			IControllerPosition controllerPosition = Controller.ReadPositions();
			if (controllerPosition.ReadOk)
			{
				int value = num3 - ControllerConfiguration.Instance.QlmApproachOffset;
				MoveTarget target = new MoveTarget
				{
					Axis = Axis.XY,
					XCoordinate = value,
					YCoordinate = num
				};
				switch (mode)
				{
				case MoveMode.None:
				case MoveMode.Get:
					MoveWithCheck(ref target);
					break;
				case MoveMode.Put:
					if (num3 < controllerPosition.XCoordinate.Value)
					{
						MoveWithCheck(ref target);
					}
					break;
				}
			}
		}
		if (data.XOffset.HasValue)
		{
			num3 += data.XOffset.Value;
		}
		if (data.YOffset.HasValue)
		{
			num += data.YOffset.Value;
		}
		MoveTarget target2 = new MoveTarget
		{
			Axis = Axis.XY,
			XCoordinate = num3,
			YCoordinate = num
		};
		ErrorCodes errorCodes2 = MoveWithCheck(ref target2);
		switch (errorCodes2)
		{
		case ErrorCodes.ArcusNotResponsive:
			if (ResetMotionControllerChecked())
			{
				errorCodes2 = MoveWithCheck(ref target2);
			}
			break;
		case ErrorCodes.LowerLimitError:
			if (!ControllerConfiguration.Instance.LowerLimitAsError)
			{
				errorCodes2 = ErrorCodes.Timeout;
			}
			break;
		}
		LogMoveError(deck, slot, errorCodes2);
		if (errorCodes2 == ErrorCodes.Success)
		{
			AtVendDoor = false;
			IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
			CurrentLocation = service.Get(deck, slot);
		}
		return errorCodes2;
	}

	public ErrorCodes MoveTo(ILocation location, MoveMode mode)
	{
		IExecutionService service = ServiceLocator.Instance.GetService<IExecutionService>();
		return MoveTo(location.Deck, location.Slot, mode, service.GetActiveContext().ContextLog);
	}

	public ErrorCodes MoveTo(ILocation location, MoveMode mode, IFormattedLog log)
	{
		return MoveTo(location.Deck, location.Slot, mode, log);
	}

	public ErrorCodes MoveTo(ILocation location, MoveMode mode, IFormattedLog log, ref OffsetMoveData data)
	{
		return MoveTo(location.Deck, location.Slot, mode, log, ref data);
	}

	public ErrorCodes MoveVend(MoveMode mode, IFormattedLog writer)
	{
		ErrorCodes errorCodes = AxesInitialized();
		if (errorCodes != ErrorCodes.Success)
		{
			LogMoveVendError(errorCodes);
			return errorCodes;
		}
		ErrorCodes errorCodes2 = CheckAndReset();
		if (errorCodes2 != ErrorCodes.Success)
		{
			LogMoveVendError(errorCodes2);
			return errorCodes2;
		}
		ErrorCodes errorCodes3 = Controller.MoveToVend(mode);
		if (ErrorCodes.ArcusNotResponsive == errorCodes3 && ResetMotionControllerChecked())
		{
			errorCodes3 = Controller.MoveToVend(mode);
		}
		LogMoveVendError(errorCodes3);
		AtVendDoor = errorCodes3 == ErrorCodes.Success;
		return errorCodes3;
	}

	public ErrorCodes InitAxes()
	{
		return InitAxes(fast: false);
	}

	public ErrorCodes InitAxes(bool fast)
	{
		if (ControllerConfiguration.Instance.RestartControllerDuringUserJobs && !CommunicationOk())
		{
			ErrorCodes errorCodes = OnReset();
			InsertCorrectionStat(errorCodes == ErrorCodes.Success);
			if (errorCodes != ErrorCodes.Success)
			{
				OnResetFailure(errorCodes);
				return errorCodes;
			}
		}
		return OnInitAxes(fast);
	}

	public ErrorCodes HomeAxis(Axis axis)
	{
		return HomeAxisChecked(axis);
	}

	public ErrorCodes Reset(bool quick)
	{
		ErrorCodes errorCodes = OnReset();
		if (errorCodes == ErrorCodes.Success)
		{
			errorCodes = OnInitAxes(quick);
		}
		if (errorCodes != ErrorCodes.Success)
		{
			OnResetFailure(errorCodes);
		}
		else
		{
			LogHelper.Instance.WithContext("RESET of motion controller returned {0}", errorCodes.ToString().ToUpper());
		}
		return errorCodes;
	}

	public bool TestAndReset()
	{
		return TestAndReset(quick: true);
	}

	public bool TestAndReset(bool quick)
	{
		if (CommunicationOk())
		{
			return true;
		}
		return Reset(quick: true) == ErrorCodes.Success;
	}

	public void Shutdown()
	{
		XAxisHomed = (YAxisHomed = false);
		Controller.OnShutdown();
	}

	public string GetPrintableLocation()
	{
		string result = "Location unknown";
		IMotionControlService service = ServiceLocator.Instance.GetService<IMotionControlService>();
		if (service.CurrentLocation != null)
		{
			result = service.CurrentLocation.ToString();
		}
		else if (service.AtVendDoor)
		{
			result = "Vend Door";
		}
		return result;
	}

	internal MotionControlService_v2()
	{
		ControllerConfiguration.Instance.AddObserver(this);
	}

	private ErrorCodes MoveWithCheck(ref MoveTarget target)
	{
		ErrorCodes errorCodes = CheckAndReset();
		if (errorCodes == ErrorCodes.Success)
		{
			return Controller.MoveToTarget(ref target);
		}
		return errorCodes;
	}

	private ErrorCodes HomeAxisChecked(Axis axis)
	{
		ErrorCodes errorCodes = CheckAndReset();
		if (errorCodes != ErrorCodes.Success)
		{
			return errorCodes;
		}
		if (Axis.Y == axis)
		{
			YAxisHomed = false;
		}
		else if (axis == Axis.X)
		{
			XAxisHomed = false;
		}
		ErrorCodes errorCodes2 = Controller.HomeAxis(axis);
		if (errorCodes2 != ErrorCodes.Success)
		{
			string msg = $"HOME {axis.ToString().ToUpper()} returned an error status {errorCodes2.ToString().ToUpper()}";
			LogHelper.Instance.WithContext(LogEntryType.Error, msg);
		}
		else
		{
			switch (axis)
			{
			case Axis.X:
				XAxisHomed = true;
				break;
			case Axis.Y:
				YAxisHomed = true;
				break;
			}
		}
		return errorCodes2;
	}

	private ErrorCodes AxesInitialized()
	{
		if (!ControllerConfiguration.Instance.ValidateControllerHomeStatus)
		{
			return ErrorCodes.Success;
		}
		int num = 0;
		if (!XAxisHomed)
		{
			num++;
			LogHelper.Instance.WithContext(toFormattedLog: false, LogEntryType.Info, "The X axis did not init.");
		}
		if (!YAxisHomed)
		{
			num++;
			LogHelper.Instance.WithContext(toFormattedLog: false, LogEntryType.Info, "The Y axis did not init.");
		}
		if (num != 0)
		{
			return ErrorCodes.MotorNotHomed;
		}
		return ErrorCodes.Success;
	}

	private ErrorCodes CheckAndReset()
	{
		using (new WithReadLock(Lock))
		{
			foreach (IMoveVeto vetoer in Vetoers)
			{
				ErrorCodes errorCodes = vetoer.CanMove();
				if (errorCodes != ErrorCodes.Success)
				{
					LogHelper.Instance.WithContext(false, LogEntryType.Error, "Move veto returned code {0}", errorCodes.ToString());
					return errorCodes;
				}
			}
		}
		CurrentLocation = null;
		AtVendDoor = false;
		return ErrorCodes.Success;
	}

	private ErrorCodes OnInitAxes(bool fast)
	{
		using ClearPickerFrontOperation clearPickerFrontOperation = new ClearPickerFrontOperation();
		if (ControllerConfiguration.Instance.ClearPickerOnHome)
		{
			clearPickerFrontOperation.Execute();
		}
		IFormattedLog contextLog = ServiceLocator.Instance.GetService<IExecutionService>().GetActiveContext().ContextLog;
		contextLog.WriteFormatted("Home the X motor.");
		int num = (fast ? 1 : 2);
		for (int i = 0; i < num; i++)
		{
			ErrorCodes errorCodes = HomeAxisChecked(Axis.X);
			if (errorCodes != ErrorCodes.Success)
			{
				return errorCodes;
			}
			if (i == 0)
			{
				MoveTarget target = new MoveTarget
				{
					Axis = Axis.X,
					XCoordinate = -200,
					YCoordinate = null
				};
				Controller.MoveToTarget(ref target);
			}
		}
		contextLog.WriteFormatted("Home the Y Motor.");
		return HomeAxisChecked(Axis.Y);
	}

	private bool ResetMotionControllerChecked()
	{
		if (!ControllerConfiguration.Instance.RestartControllerDuringUserJobs)
		{
			return false;
		}
		IExecutionContext activeContext = ServiceLocator.Instance.GetService<IExecutionService>().GetActiveContext();
		HardwareCorrectionEventArgs e = new HardwareCorrectionEventArgs(HardwareCorrectionStatistic.Arcus);
		activeContext?.HardwareCorrectionStart(e);
		e.CorrectionOk = Reset(quick: true) == ErrorCodes.Success;
		activeContext?.HardwareCorrectionEnd(e);
		InsertCorrectionStat(activeContext, e.CorrectionOk);
		return e.CorrectionOk;
	}

	private void InsertCorrectionStat(bool ok)
	{
		IExecutionContext activeContext = ServiceLocator.Instance.GetService<IExecutionService>().GetActiveContext();
		InsertCorrectionStat(activeContext, ok);
	}

	private void InsertCorrectionStat(IExecutionContext activeContext, bool ok)
	{
		if (activeContext != null)
		{
			ServiceLocator.Instance.GetService<IHardwareCorrectionStatisticService>().Insert(HardwareCorrectionStatistic.Arcus, activeContext, ok);
		}
	}

	private void LogMoveVendError(ErrorCodes error)
	{
		if (error != ErrorCodes.Success)
		{
			LogHelper.Instance.WithContext(LogEntryType.Error, "MOVEVEND returned an error status {0}", error.ToString().ToUpper());
		}
	}

	private void LogMoveError(int deck, int slot, ErrorCodes error)
	{
		if (error != ErrorCodes.Success)
		{
			LogHelper.Instance.WithContext(LogEntryType.Error, "MOVE Deck = {0} Slot = {1} returned an error status {2}", deck, slot, error.ToString().ToUpper());
		}
	}

	private ErrorCodes OnReset()
	{
		Shutdown();
		if (!Controller.OnResetDeviceDriver())
		{
			return ErrorCodes.ArcusNotResponsive;
		}
		Initialize();
		if (Controller.CommunicationOk())
		{
			return ErrorCodes.Success;
		}
		return ErrorCodes.ArcusNotResponsive;
	}

	private void OnResetFailure(ErrorCodes failure)
	{
		LogHelper.Instance.WithContext(false, LogEntryType.Error, "Reset of motion controller returned error {0}", failure);
	}
}
