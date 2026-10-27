namespace Redbox.HAL.Component.Model;

public interface IFormattedLog
{
	void WriteFormatted(string msg);

	void WriteFormatted(string msg, LogEntryType type);

	void WriteFormatted(string format, params object[] stuff);

	void WriteFormatted(LogEntryType type, string format, params object[] stuff);
}
