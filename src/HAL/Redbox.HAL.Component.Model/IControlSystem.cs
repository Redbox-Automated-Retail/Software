namespace Redbox.HAL.Component.Model;

public interface IControlSystem
{
	VendDoorState VendDoorState { get; }

	TrackState TrackState { get; }

	IControlResponse Initialize();

	bool Shutdown();

	IControlResponse SetAudio(AudioChannelState newState);

	IControlResponse ToggleRingLight(bool on, int? sleepAfter);

	IControlResponse VendDoorRent();

	IControlResponse VendDoorClose();

	VendDoorState ReadVendDoorPosition();

	IControlResponse TrackOpen();

	IControlResponse TrackClose();

	ErrorCodes TrackCycle();

	void TimedExtend();

	void TimedExtend(int timeout);

	IControlResponse ExtendArm();

	IControlResponse ExtendArm(int timeout);

	IControlResponse RetractArm();

	IControlResponse SetFinger(GripperFingerState state);

	ErrorCodes Center(CenterDiskMethod method);

	IBoardVersionResponse GetBoardVersion(ControlBoards board);

	IControlSystemRevision GetRevision();

	IReadInputsResult<PickerInputs> ReadPickerInputs();

	IReadInputsResult<AuxInputs> ReadAuxInputs();

	void LogPickerSensorState();

	void LogPickerSensorState(LogEntryType type);

	void LogInputs(ControlBoards board);

	void LogInputs(ControlBoards board, LogEntryType type);

	IControlResponse SetSensors(bool on);

	IPickerSensorReadResult ReadPickerSensors();

	IPickerSensorReadResult ReadPickerSensors(bool forceTrackClose);

	IControlResponse StartRollerIn();

	IControlResponse StartRollerOut();

	IControlResponse StopRoller();

	IControlResponse SetRollerState(RollerState state);

	IControlResponse RollerToPosition(RollerPosition position);

	IControlResponse RollerToPosition(RollerPosition position, int opTimeout);

	IControlResponse RollerToPosition(RollerPosition position, int opTimeout, bool logSensors);

	QlmStatus GetQlmStatus();

	ErrorCodes EngageQlm(IFormattedLog log);

	ErrorCodes EngageQlm(bool home, IFormattedLog log);

	ErrorCodes DisengageQlm(IFormattedLog log);

	ErrorCodes DisengageQlm(bool home, IFormattedLog log);

	IControlResponse LockQlmDoor();

	IControlResponse UnlockQlmDoor();

	IControlResponse DropQlm();

	IControlResponse LiftQlm();

	IControlResponse HaltQlmLifter();
}
