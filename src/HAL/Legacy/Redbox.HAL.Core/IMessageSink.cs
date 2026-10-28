namespace Redbox.HAL.Core;

public interface IMessageSink
{
	bool Send(string message);
}
