namespace Redbox.HAL.Component.Model;

public interface IPickerSensorReadResult
{
	int BlockedCount { get; }

	ErrorCodes Error { get; }

	bool[] Sensors { get; }

	bool IsFull { get; }

	void LogSensorState();

	void LogSensorState(LogEntryType type);
}
