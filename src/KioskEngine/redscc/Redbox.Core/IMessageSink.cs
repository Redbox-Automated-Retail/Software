namespace Redbox.Core;

public interface IMessageSink
{
	bool Send(string message);
}
