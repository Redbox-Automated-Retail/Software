namespace Redbox.HAL.Component.Model;

public interface IFormattedLog
{
	void WriteFormatted(string msg);

	void WriteFormatted(string format, params object[] stuff);
}
