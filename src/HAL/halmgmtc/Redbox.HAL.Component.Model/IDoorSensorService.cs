namespace Redbox.HAL.Component.Model;

public interface IDoorSensorService : IMoveVeto
{
	bool SensorsEnabled { get; }

	bool SoftwareOverride { get; set; }

	DoorSensorResult Query();

	DoorSensorResult QueryStateForDisplay();
}
