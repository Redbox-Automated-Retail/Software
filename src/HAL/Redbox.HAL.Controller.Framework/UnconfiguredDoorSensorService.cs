using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal sealed class UnconfiguredDoorSensorService : IDoorSensorService, IMoveVeto
{
	public bool SensorsEnabled => false;

	public bool SoftwareOverride { get; set; }

	public ErrorCodes CanMove()
	{
		return ErrorCodes.Success;
	}

	public DoorSensorResult Query()
	{
		return DoorSensorResult.Ok;
	}

	public DoorSensorResult QueryStateForDisplay()
	{
		return DoorSensorResult.Ok;
	}

	internal UnconfiguredDoorSensorService()
	{
	}
}
