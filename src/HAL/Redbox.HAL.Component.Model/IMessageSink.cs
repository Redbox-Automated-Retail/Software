namespace Redbox.HAL.Component.Model;

public interface IMessageSink
{
	bool Send(string message);
}
