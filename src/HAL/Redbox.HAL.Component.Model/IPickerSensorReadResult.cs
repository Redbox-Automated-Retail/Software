namespace Redbox.HAL.Component.Model;

public interface IPickerSensorReadResult : IReadInputsResult<PickerInputs>
{
	bool IsFull { get; }

	int BlockedCount { get; }
}
