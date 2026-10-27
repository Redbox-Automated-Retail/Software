using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal abstract class AbstractRedboxControlSystem
{
	protected readonly IRuntimeService RuntimeService;

	protected AbstractRedboxControlSystem(IRuntimeService rts)
	{
		RuntimeService = rts;
	}

	internal abstract CoreResponse Initialize();

	internal abstract bool Shutdown();

	internal abstract CoreResponse SetAudio(AudioChannelState newState);

	internal abstract CoreResponse SetRinglight(bool on);

	internal abstract CoreResponse SetPickerSensors(bool on);

	internal abstract CoreResponse SetFinger(GripperFingerState state);

	internal abstract CoreResponse SetRoller(RollerState state);

	internal abstract CoreResponse RollerToPosition(RollerPosition position, int opTimeout);

	internal abstract CoreResponse TimedArmExtend(int timeout);

	internal abstract CoreResponse ExtendArm(int timeout);

	internal abstract CoreResponse RetractArm(int timeout);

	internal abstract CoreResponse SetTrack(TrackState state);

	internal abstract CoreResponse SetVendDoor(VendDoorState state);

	internal abstract VendDoorState ReadVendDoorState();

	internal abstract QlmStatus GetQlmStatus();

	internal abstract CoreResponse OnQlm(QlmOperation op);

	internal abstract ReadAuxInputsResult ReadAuxInputs();

	internal abstract ReadPickerInputsResult ReadPickerInputs();

	internal abstract BoardVersionResponse GetBoardVersion(ControlBoards board);

	internal abstract IControlSystemRevision GetRevision();
}
