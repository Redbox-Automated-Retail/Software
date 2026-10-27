using System;
using System.Text;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;

namespace Redbox.HAL.Client.Services;

public sealed class ClientControlSystem : IControlSystem
{
	private readonly HardwareService Service;

	private readonly byte[] ReadImmediateInstruction;

	public VendDoorState VendDoorState
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public TrackState TrackState
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public IControlResponse Initialize()
	{
		return From("SERIALBOARD RESET");
	}

	public bool Shutdown()
	{
		using InstructionHelper instructionHelper = new InstructionHelper(Service);
		return instructionHelper.ExecuteGeneric("SERIALBOARD CLOSEPORT").Equals("SUCCESS", StringComparison.CurrentCultureIgnoreCase);
	}

	public IControlResponse SetAudio(AudioChannelState newState)
	{
		return From($"AUDIO {newState.ToString()}");
	}

	public IControlResponse ToggleRingLight(bool on, int? sleepAfter)
	{
		return From(string.Format("CONTROLSYSTEM RINGLIGHT{0}", on ? "ON" : "OFF"));
	}

	public IControlResponse VendDoorRent()
	{
		return From("VENDDOOR RENT");
	}

	public IControlResponse VendDoorClose()
	{
		return From("VENDDOOR CLOSE");
	}

	public VendDoorState ReadVendDoorPosition()
	{
		using InstructionHelper instructionHelper = new InstructionHelper(Service);
		string value = instructionHelper.ExecuteGeneric("VENDDOOR STATUS");
		return (!string.IsNullOrEmpty(value)) ? Enum<VendDoorState>.ParseIgnoringCase(value, VendDoorState.Unknown) : VendDoorState.Unknown;
	}

	public IControlResponse TrackOpen()
	{
		return From("TRACK OPEN");
	}

	public IControlResponse TrackClose()
	{
		return From("TRACK CLOSE");
	}

	public ErrorCodes TrackCycle()
	{
		using InstructionHelper instructionHelper = new InstructionHelper(Service);
		return instructionHelper.ExecuteErrorCode("TRACK CYCLE");
	}

	public void TimedExtend()
	{
	}

	public void TimedExtend(int timeout)
	{
	}

	public IControlResponse ExtendArm()
	{
		return From("GRIPPER EXTEND");
	}

	public IControlResponse ExtendArm(int timeout)
	{
		throw new NotImplementedException();
	}

	public IControlResponse RetractArm()
	{
		return From("GRIPPER RETRACT");
	}

	public IControlResponse SetFinger(GripperFingerState state)
	{
		string instruction = string.Format("GRIPPER {0}", (GripperFingerState.Closed == state) ? "CLOSE" : state.ToString());
		return From(instruction);
	}

	public ErrorCodes Center(CenterDiskMethod method)
	{
		using InstructionHelper instructionHelper = new InstructionHelper(Service);
		return instructionHelper.ExecuteErrorCode("CONTROLSYSTEM CENTER");
	}

	public IBoardVersionResponse GetBoardVersion(ControlBoards board)
	{
		throw new NotImplementedException();
	}

	public IControlSystemRevision GetRevision()
	{
		throw new NotImplementedException();
	}

	public IReadInputsResult<PickerInputs> ReadPickerInputs()
	{
		return new ClientReadPickerInputsResult(Service);
	}

	public IReadInputsResult<AuxInputs> ReadAuxInputs()
	{
		return new ClientReadAuxInputsResult(Service);
	}

	public void LogPickerSensorState()
	{
	}

	public void LogPickerSensorState(LogEntryType type)
	{
	}

	public void LogInputs(ControlBoards board)
	{
	}

	public void LogInputs(ControlBoards board, LogEntryType type)
	{
	}

	public IControlResponse SetSensors(bool on)
	{
		return From(string.Format("CONTROLSYSTEM PICKERSENSORS{0}", on ? "ON" : "OFF"));
	}

	public IPickerSensorReadResult ReadPickerSensors()
	{
		throw new NotImplementedException("ReadPickerSensors");
	}

	public IPickerSensorReadResult ReadPickerSensors(bool f)
	{
		string statement = "SENSOR READ PICKER-SENSOR=1..6";
		if (!Service.ExecuteImmediate(statement, out var job).Success)
		{
			return new ClientPickerSensorReadResult(ErrorCodes.ServiceChannelError);
		}
		if (!job.GetStack(out var stack).Success)
		{
			return new ClientPickerSensorReadResult(ErrorCodes.ServiceChannelError);
		}
		if (stack.Count == 0)
		{
			return new ClientPickerSensorReadResult(ErrorCodes.CommunicationError);
		}
		if (int.Parse(stack.Pop()) >= 6)
		{
			return new ClientPickerSensorReadResult(stack);
		}
		return new ClientPickerSensorReadResult(ErrorCodes.CommunicationError);
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

	public IControlResponse SetRollerState(RollerState s)
	{
		return From($"ROLLER {s.ToString().ToUpper()}");
	}

	public IControlResponse RollerToPosition(RollerPosition position)
	{
		return RollerToPosition(position, 5000);
	}

	public IControlResponse RollerToPosition(RollerPosition position, int opTimeout)
	{
		return RollerToPosition(position, opTimeout, logSensors: false);
	}

	public IControlResponse RollerToPosition(RollerPosition position, int opTimeout, bool logSensors)
	{
		int num = (int)position;
		return From($"ROLLER POS={num.ToString()} TIMEOUT={opTimeout} WAIT=TRUE");
	}

	public QlmStatus GetQlmStatus()
	{
		throw new NotImplementedException();
	}

	public ErrorCodes EngageQlm(IFormattedLog log)
	{
		return EngageQlm(home: true, log);
	}

	public ErrorCodes EngageQlm(bool home, IFormattedLog log)
	{
		using InstructionHelper instructionHelper = new InstructionHelper(Service);
		return instructionHelper.ExecuteErrorCode("QLM ENGAGE");
	}

	public ErrorCodes DisengageQlm(IFormattedLog log)
	{
		return DisengageQlm(home: true, log);
	}

	public ErrorCodes DisengageQlm(bool home, IFormattedLog log)
	{
		using InstructionHelper instructionHelper = new InstructionHelper(Service);
		return instructionHelper.ExecuteErrorCode("QLM DISENGAGE");
	}

	public IControlResponse LockQlmDoor()
	{
		throw new NotImplementedException();
	}

	public IControlResponse UnlockQlmDoor()
	{
		throw new NotImplementedException();
	}

	public IControlResponse DropQlm()
	{
		return From("CONTROLSYSTEM DROPQLM");
	}

	public IControlResponse LiftQlm()
	{
		return From("CONTROLSYSTEM LIFTQLM");
	}

	public IControlResponse HaltQlmLifter()
	{
		return From("CONTROLSYSTEM HALTQLMOPERATION");
	}

	public ClientControlSystem(HardwareService s)
	{
		Service = s;
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("CLEAR");
		stringBuilder.AppendLine(" SENSOR READ PICKER-SENSOR=1..6");
		ReadImmediateInstruction = Encoding.ASCII.GetBytes(stringBuilder.ToString());
	}

	private IControlResponse From(string instruction)
	{
		using InstructionHelper instructionHelper = new InstructionHelper(Service);
		return instructionHelper.ExecuteWithResponse(instruction);
	}
}
