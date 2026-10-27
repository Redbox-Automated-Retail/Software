namespace Redbox.HAL.Client;

internal class NullSink : IClientOutputSink
{
	public void WriteMessage(string msg)
	{
	}

	public void WriteMessage(string fmt, params object[] stuff)
	{
	}
}
