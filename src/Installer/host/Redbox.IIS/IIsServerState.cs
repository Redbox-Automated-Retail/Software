namespace Redbox.IIS;

public enum IIsServerState
{
	Starting = 1,
	Started,
	Stopping,
	Stopped,
	Pausing,
	Paused,
	Continuing
}
