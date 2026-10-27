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

	ErrorCodes MoveTo(int deck, int slot, MoveMode mode);

	ErrorCodes MoveTo(int deck, int slot, MoveMode mode, IFormattedLog log);

	ErrorCodes MoveTo(int deck, int slot, MoveMode mode, IFormattedLog log, ref OffsetMoveData data);

	ErrorCodes MoveTo(ILocation location, MoveMode mode);

	ErrorCodes MoveTo(ILocation location, MoveMode mode, IFormattedLog log);

	ErrorCodes MoveTo(ILocation location, MoveMode mode, IFormattedLog log, ref OffsetMoveData data);

	ErrorCodes MoveVend(MoveMode mode, IFormattedLog log);

	ErrorCodes InitAxes();

	ErrorCodes InitAxes(bool quick);

	ErrorCodes HomeAxis(Axis axis);

	ErrorCodes Reset(bool quick);

	bool TestAndReset();

	bool TestAndReset(bool quick);

	void Shutdown();

	string GetPrintableLocation();
}
