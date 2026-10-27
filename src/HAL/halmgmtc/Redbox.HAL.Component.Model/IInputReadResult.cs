namespace Redbox.HAL.Component.Model;

public interface IInputReadResult
{
	ErrorCodes Error { get; }

	string[] Result { get; }

	void Log();

	void Log(LogEntryType type);
}
