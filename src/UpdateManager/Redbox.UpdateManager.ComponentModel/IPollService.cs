namespace Redbox.UpdateManager.ComponentModel;

public interface IPollService
{
	ErrorList ServerPoll();

	ErrorList ServerPoll(bool suppressHealthUpdate);

	ErrorList ServerPoll(IPoll poll);
}
