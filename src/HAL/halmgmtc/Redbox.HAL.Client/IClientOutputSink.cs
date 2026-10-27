namespace Redbox.HAL.Client;

public interface IClientOutputSink
{
	void WriteMessage(string msg);

	void WriteMessage(string fmt, params object[] stuff);
}
