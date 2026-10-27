namespace Redbox.HAL.Component.Model;

public interface IControlSystem
{
	VendDoorState VendDoorState { get; }

	TrackState TrackState { get; }

	IControlResponse Initialize();

	void Shutdown();

	IControlResponse SetAudio(AudioChannelState newState);

	void ToggleRingLight(bool on, int? sleepAfter);

	IControlResponse VendDoorRent();

	IControlResponse VendDoorClose();

	VendDoorState ReadVendDoorPosition();

	IControlResponse TrackOpen();

	IControlResponse TrackClose();

	IControlResponse TrackCycle();

	void TimedExtend();

	void TimedExtend(int timeout);

	IControlResponse ExtendArm();

	IControlResponse ExtendArm(int timeout);

	IControlResponse RetractArm();

	IControlResponse SetFinger(GripperFingerState state);

	IControlResponse ToggleJunction(PickerJunction junction, JunctionState state);

	ErrorCodes Center(CenterDiskMethod method);

	string GetBoardVersion(ControlBoards board);

	string[] GetBoardVersions();

	string[] ReadInputs(ControlBoards board);

	void LogPickerSensorState();

	void LogPickerSensorState(LogEntryType type);

	void LogInputs(ControlBoards board);

	void LogInputs(ControlBoards board, LogEntryType type);

	IControlResponse SetSensors(bool on);

	IPickerSensorReadResult ReadSensors();

	IPickerSensorReadResult ReadImmediateSensors();

	IControlResponse StopRoller();

	IControlResponse StartRollerIn();

	IControlResponse StartRollerOut();

	IControlResponse RollerToPosition(RollerPosition position);

	IControlResponse RollerToPosition(RollerPosition position, int opTimeout);

	IControlResponse RollerToPosition(RollerPosition position, int opTimeout, bool logSensors);
}
