namespace Redbox.HAL.Component.Model;

public interface IMotionControlService
{
	ILocation CurrentLocation { get; }

	bool AtVendDoor { get; }

	bool IsInitialized { get; }

	void AddVeto(IMoveVeto veto);

	void RemoveVeto(IMoveVeto veto);

	bool Initialize();

	bool CommunicationOk();

	IControllerPosition ReadPositions();

	IMotionControlLimitResponse ReadLimits();

	ErrorCodes MoveAbsolute(Axis axis, int? xunits, int? yunits, bool checkSensors);

	ErrorCodes MoveTo(int deck, int slot, MoveMode mode, IFormattedLog log, ref OffsetMoveData data);

	ErrorCodes MoveTo(int deck, int slot, MoveMode mode, IFormattedLog log);

	ErrorCodes MoveTo(ILocation location, MoveMode mode, IFormattedLog log, ref OffsetMoveData data);

	ErrorCodes MoveTo(ILocation location, MoveMode mode, IFormattedLog log);

	ErrorCodes MoveVend(MoveMode mode, IFormattedLog log);

	ErrorCodes InitAxes(IFormattedLog log);

	ErrorCodes InitAxesFast(IFormattedLog log);

	ErrorCodes HomeAxis(Axis axis, IFormattedLog log);

	bool Reset(IFormattedLog log, bool quick);

	void Shutdown();
}
