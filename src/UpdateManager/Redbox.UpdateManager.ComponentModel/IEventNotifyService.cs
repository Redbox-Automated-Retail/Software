namespace Redbox.UpdateManager.ComponentModel;

public interface IEventNotifyService
{
	ErrorList AddEvent(string name, string description);

	ErrorList EventStart(string name);

	ErrorList EventErrored(string name, string code, string description, string details);

	ErrorList EventComplete(string name);

	ErrorList Exit();
}
