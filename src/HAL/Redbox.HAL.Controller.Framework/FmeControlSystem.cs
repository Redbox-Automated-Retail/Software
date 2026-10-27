using System;
using System.IO.Ports;
using System.Text;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal sealed class FmeControlSystem : AbstractRedboxControlSystem, ICoreCommandExecutor
{
	private class FmeControllerRevision : IControlSystemRevision
	{
		public bool Success { get; internal set; }

		public IBoardVersionResponse[] Responses { get; internal set; }

		public string Revision => "A";
	}

	private readonly ControlBoards[] ControllerBoards = new ControlBoards[3]
	{
		ControlBoards.Serial,
		ControlBoards.Picker,
		ControlBoards.Aux
	};

	private readonly ICommPort Port;

	private const int LifterTimeout = 120000;

	public CoreResponse ExecuteControllerCommand(CommandType type)
	{
		return SendCommand(CoreCommand.Create(type, null, Port));
	}

	public CoreResponse ExecuteControllerCommand(CommandType type, int? timeout)
	{
		return SendCommand(CoreCommand.Create(type, timeout, Port));
	}

	internal override CoreResponse Initialize()
	{
		CoreResponse result = ExecuteControllerCommand(CommandType.Reset);
		RuntimeService.Wait(2000);
		SetAudio(AudioChannelState.Off);
		return result;
	}

	internal override bool Shutdown()
	{
		return Port.Close();
	}

	internal override CoreResponse SetAudio(AudioChannelState newState)
	{
		return ExecuteControllerCommand((AudioChannelState.On == newState) ? CommandType.AudioOn : CommandType.AudioOff);
	}

	internal override CoreResponse SetRinglight(bool on)
	{
		CommandType type = (on ? CommandType.RinglightOn : CommandType.RinglightOff);
		return ExecuteControllerCommand(type);
	}

	internal override CoreResponse SetPickerSensors(bool on)
	{
		return ExecuteControllerCommand(on ? CommandType.SensorBarOn : CommandType.SensorBarOff);
	}

	internal override CoreResponse SetFinger(GripperFingerState state)
	{
		CommandType command = CommandType.GripperClose;
		switch (state)
		{
		case GripperFingerState.Closed:
			command = CommandType.GripperClose;
			break;
		case GripperFingerState.Rent:
			command = ((!ControllerConfiguration.Instance.EnableSecureDiskValidator) ? CommandType.GripperRent : CommandType.GripperOpen);
			break;
		case GripperFingerState.Open:
			if (ControllerConfiguration.Instance.EnableSecureDiskValidator)
			{
				return CoreResponse.TimedOutResponse;
			}
			command = CommandType.GripperOpen;
			break;
		}
		CoreResponse coreResponse = OnRetryable(command, 2, 5000);
		LogError(coreResponse, $"RetryableFingerFunction {state}", ControlBoards.Picker);
		return coreResponse;
	}

	internal override CoreResponse SetRoller(RollerState state)
	{
		CommandType type = CommandType.RollerStop;
		switch (state)
		{
		case RollerState.None:
			throw new ArgumentException("Cannot be none");
		case RollerState.In:
			type = CommandType.RollerIn;
			break;
		case RollerState.Out:
			type = CommandType.RollerOut;
			break;
		case RollerState.Stop:
			type = CommandType.RollerStop;
			break;
		}
		CoreResponse coreResponse = ExecuteControllerCommand(type);
		LogError(coreResponse, $"Roller{state}", ControlBoards.Picker);
		return coreResponse;
	}

	internal override CoreResponse RollerToPosition(RollerPosition position, int opTimeout)
	{
		CommandType type = CommandType.RollerToPos1;
		switch (position)
		{
		case RollerPosition.Position1:
			type = CommandType.RollerToPos1;
			break;
		case RollerPosition.Position2:
			type = CommandType.RollerToPos2;
			break;
		case RollerPosition.Position3:
			type = CommandType.RollerToPos3;
			break;
		case RollerPosition.Position4:
			type = CommandType.RollerToPos4;
			break;
		case RollerPosition.Position5:
			type = CommandType.RollerToPos5;
			break;
		case RollerPosition.Position6:
			type = CommandType.RollerToPos6;
			break;
		}
		return ExecuteControllerCommand(type, opTimeout);
	}

	internal override CoreResponse TimedArmExtend(int timeout)
	{
		try
		{
			LogHelper.Instance.Log(LogEntryType.Debug, "[FmeControlSystem] ExtendGripperArmForTime {0} ms", timeout);
			CoreResponse result = ExecuteControllerCommand(CommandType.ExtendGripperArmForTime);
			RuntimeService.Wait(timeout);
			return result;
		}
		finally
		{
			ExecuteControllerCommand(CommandType.GripperExtendHalt);
		}
	}

	internal override CoreResponse ExtendArm(int timeout)
	{
		CoreResponse coreResponse = ExecuteControllerCommand(CommandType.GripperExtend, timeout);
		LogError(coreResponse, "SetGripperArm( Extend )", ControlBoards.Picker);
		if (coreResponse.TimedOut)
		{
			ReadPickerInputsResult readPickerInputsResult = ReadPickerInputs();
			if (readPickerInputsResult.Success && readPickerInputsResult.IsInputActive(PickerInputs.Extend))
			{
				LogHelper.Instance.WithContext("GripperExtend status returned timeout; however, picker sensors read shows the forward sensor reached.");
				readPickerInputsResult.Log();
				coreResponse.Error = ErrorCodes.Success;
			}
		}
		return coreResponse;
	}

	internal override CoreResponse RetractArm(int timeout)
	{
		CoreResponse coreResponse = OnRetryable(CommandType.GripperRetract, 2, timeout);
		LogError(coreResponse, "SetGripperArm( RETRACT )", ControlBoards.Picker);
		if (coreResponse.TimedOut)
		{
			ReadPickerInputsResult readPickerInputsResult = ReadPickerInputs();
			if (readPickerInputsResult.Success && readPickerInputsResult.IsInputActive(PickerInputs.Retract))
			{
				LogHelper.Instance.WithContext("GripperRetract status returned timeout; however, picker sensors read shows the sensor tripped.");
				readPickerInputsResult.Log();
			}
		}
		return coreResponse;
	}

	internal override CoreResponse SetTrack(TrackState state)
	{
		CommandType command = ((state == TrackState.Open) ? CommandType.TrackOpen : CommandType.TrackClose);
		CoreResponse coreResponse = OnRetryable(command, 2, 5000);
		LogError(coreResponse, $"Track {state.ToString()}", ControlBoards.Picker);
		return coreResponse;
	}

	internal override CoreResponse SetVendDoor(VendDoorState state)
	{
		CoreResponse coreResponse = null;
		int timeout = 5500;
		coreResponse = ((VendDoorState.Rent != state) ? OnCloseVendDoor(timeout) : OnRetryable(CommandType.VendDoorRent, 1, timeout));
		LogError(coreResponse, $"VendDoor {state}", ControlBoards.Aux);
		return coreResponse;
	}

	internal override VendDoorState ReadVendDoorState()
	{
		ReadAuxInputsResult readAuxInputsResult = ReadAuxInputs();
		if (!readAuxInputsResult.Success)
		{
			LogHelper.Instance.WithContext(false, LogEntryType.Error, "Read AUX sensors returned error {0}", readAuxInputsResult.Error.ToString().ToUpper());
			return VendDoorState.Unknown;
		}
		if (readAuxInputsResult.IsInputActive(AuxInputs.VendDoorClosed))
		{
			return VendDoorState.Closed;
		}
		if (readAuxInputsResult.IsInputActive(AuxInputs.VendDoorRent))
		{
			return VendDoorState.Rent;
		}
		return VendDoorState.Unknown;
	}

	internal override QlmStatus GetQlmStatus()
	{
		ReadAuxInputsResult readAuxInputsResult = ReadAuxInputs();
		if (!readAuxInputsResult.Success)
		{
			return QlmStatus.AuxNotResponsive;
		}
		if (!readAuxInputsResult.IsInputActive(AuxInputs.QlmPresence))
		{
			return QlmStatus.Empty;
		}
		if (!readAuxInputsResult.IsInputActive(AuxInputs.QlmUp))
		{
			return QlmStatus.Disengaged;
		}
		return QlmStatus.Engaged;
	}

	internal override CoreResponse OnQlm(QlmOperation op)
	{
		CoreResponse coreResponse = null;
		switch (op)
		{
		case QlmOperation.None:
			throw new ArgumentException("[FmeControlSystem] OnQlm: operation cannot be none");
		case QlmOperation.Engage:
			coreResponse = ExecuteControllerCommand(CommandType.QlmEngage, 120000);
			break;
		case QlmOperation.Disengage:
			coreResponse = ExecuteControllerCommand(CommandType.QlmDisengage, 120000);
			break;
		case QlmOperation.Drop:
			coreResponse = ExecuteControllerCommand(CommandType.QlmDrop);
			break;
		case QlmOperation.Lift:
			coreResponse = ExecuteControllerCommand(CommandType.QlmLift);
			break;
		case QlmOperation.Halt:
			coreResponse = ExecuteControllerCommand(CommandType.QlmHalt);
			break;
		case QlmOperation.LockDoor:
			coreResponse = ExecuteControllerCommand(CommandType.QlmDoorLock);
			break;
		case QlmOperation.UnlockDoor:
			coreResponse = ExecuteControllerCommand(CommandType.QlmDoorUnlock);
			break;
		}
		LogError(coreResponse, op.ToString().ToUpper(), ControlBoards.Aux);
		return coreResponse;
	}

	internal override ReadAuxInputsResult ReadAuxInputs()
	{
		return new ReadAuxInputsResult(ExecuteControllerCommand(CommandType.AuxSensorsRead));
	}

	internal override ReadPickerInputsResult ReadPickerInputs()
	{
		return new ReadPickerInputsResult(ExecuteControllerCommand(CommandType.ReadPickerInputs));
	}

	internal override BoardVersionResponse GetBoardVersion(ControlBoards board)
	{
		CommandType type = CommandType.Version101;
		switch (board)
		{
		case ControlBoards.Aux:
			type = CommandType.Version002;
			break;
		case ControlBoards.Picker:
			type = CommandType.Version001;
			break;
		case ControlBoards.Serial:
			type = CommandType.Version101;
			break;
		}
		return new BoardVersionResponse(board, ExecuteControllerCommand(type));
	}

	internal override IControlSystemRevision GetRevision()
	{
		IBoardVersionResponse[] array = new IBoardVersionResponse[ControllerBoards.Length];
		int num = 0;
		for (int i = 0; i < ControllerBoards.Length; i++)
		{
			array[i] = GetBoardVersion(ControllerBoards[i]);
			if (!array[i].ReadSuccess)
			{
				num++;
			}
		}
		return new FmeControllerRevision
		{
			Responses = array,
			Success = (num == 0)
		};
	}

	internal FmeControlSystem(IRuntimeService rts)
		: base(rts)
	{
		SerialPort port = new SerialPort(ControllerConfiguration.Instance.ControllerPortName, 9600, Parity.None, 8, StopBits.One);
		IPortManagerService service = ServiceLocator.Instance.GetService<IPortManagerService>();
		Port = service.Create(port);
		Port.WriteTerminator = new byte[1] { 13 };
		Port.ValidateResponse = delegate(IChannelResponse response)
		{
			string text = Encoding.ASCII.GetString(response.RawResponse);
			return !string.IsNullOrEmpty(text) && (text.IndexOf("OK") != -1 || text.IndexOf("EER") != -1);
		};
	}

	private CoreResponse OnRetryable(CommandType command, int retryCount, int timeout)
	{
		CoreResponse coreResponse = null;
		for (int i = 0; i < retryCount; i++)
		{
			coreResponse = ExecuteControllerCommand(command, timeout);
			if (coreResponse.Success || coreResponse.CommError)
			{
				return coreResponse;
			}
		}
		return coreResponse;
	}

	private CoreResponse SendCommand(CoreCommand command)
	{
		if (command.Port == null || !command.Port.Open())
		{
			LogHelper.Instance.WithContext(LogEntryType.Error, "Unable to open control port {0}", command.Port.DisplayName);
			return CoreResponse.CommErrorResponse;
		}
		try
		{
			return command.Execute();
		}
		catch (Exception ex)
		{
			LogHelper.Instance.Log(LogEntryType.Error, $"[FmeControlSystem] Send command on port {command.Port.DisplayName} caught an exception.", ex);
			return CoreResponse.CommErrorResponse;
		}
	}

	private CoreResponse OnCloseVendDoor(int timeout)
	{
		CoreResponse coreResponse = null;
		for (int i = 0; i < 2; i++)
		{
			coreResponse = ExecuteControllerCommand(CommandType.VendDoorClose, timeout);
			if (coreResponse.CommError)
			{
				return coreResponse;
			}
			if (coreResponse.TimedOut)
			{
				OnCloseBackoff();
			}
			else if (ReadVendDoorState() == VendDoorState.Closed || MoveToCloseSensor())
			{
				return coreResponse;
			}
		}
		return coreResponse;
	}

	private bool MoveToCloseSensor()
	{
		if (!ControllerConfiguration.Instance.MoveVendDoorToAuxSensor)
		{
			return true;
		}
		OnCloseBackoff();
		CoreResponse coreResponse = ExecuteControllerCommand(CommandType.VendDoorClose, 5000);
		if (coreResponse.CommError)
		{
			return false;
		}
		RuntimeService.SpinWait(100);
		return !coreResponse.TimedOut;
	}

	private void OnCloseBackoff()
	{
		RuntimeService.Wait(200);
		ExecuteControllerCommand(CommandType.UnknownVendDoorCloseCommand);
		RuntimeService.Wait(100);
		ExecuteControllerCommand(CommandType.VendDoorKillCommand);
		RuntimeService.Wait(100);
	}

	private void LogError(CoreResponse response, string command, ControlBoards board)
	{
		if (response.Success)
		{
			return;
		}
		LogHelper.Instance.WithContext(false, LogEntryType.Error, "{0} returned error status {1}.", command, response.Error.ToString().ToUpper());
		if (!response.CommError)
		{
			if (ControlBoards.Aux == board)
			{
				ReadAuxInputs().Log(LogEntryType.Error);
			}
			else if (ControlBoards.Picker == board)
			{
				ReadPickerInputs().Log(LogEntryType.Error);
			}
		}
	}
}
